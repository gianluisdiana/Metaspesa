import logging
import re
from typing import override

from bs4 import BeautifulSoup, Tag

from application.abstractions import MarketWebScraper
from config import ScraperSettings
from domain import Product, Subcategory
from infrastructure.web_driver import WebDriver


class AlcampoWebScraper(MarketWebScraper):
    def __init__(self, driver: WebDriver, settings: ScraperSettings) -> None:
        super().__init__()
        self.__driver = driver
        self.__skipped_categories: set[str] = set(settings.skipped_categories)
        self.__category_urls: dict[str, str] = {}
        self.__logger = logging.getLogger(self.__class__.__name__)
        self.__url = "https://www.compraonline.alcampo.es"

    @override
    async def set_location(self, postal_code: str) -> None:
        await self.__driver.get(self.__url)

        await self.__driver.wait_and_click_css("#onetrust-reject-all-handler")
        try:
            await self.__driver.wait_for_invisibility_css(
                "#onetrust-reject-all-handler"
            )
        except Exception:
            pass

        await self.__driver.wait_and_click_xpath(
            '//button[@data-test="popup-banner-close-button"]', timeout=10
        )
        await self.__driver.wait_and_click_css("._button--fill_ftyis_140")
        await self.__driver.wait_and_click_css("._button--primary_ftyis_42 > span")

        await self.__driver.wait_for_presence_xpath(
            '//input[@data-test="search-input"]'
        )
        await self.__driver.wait_and_send_keys_xpath(
            '//input[@data-test="search-input"]', postal_code
        )

        await self.__driver.wait_and_click_xpath(
            f'//span[contains(text(), "{postal_code}")]'
        )
        await self.__driver.wait_and_click_xpath('//span[text()="Confirmar ubicación"]')
        await self.__driver.wait_and_click_xpath(
            '//button[@data-test="choose-delivery-method-submit"]'
        )

        await self.__driver.refresh()
        self.__logger.info("Location set to postal code %s", postal_code)

    @override
    async def get_categories(self) -> list[str]:
        await self.__driver.wait_and_click_css(".dropdown-item-button")
        await self.__driver.wait_and_click_xpath("//span[text()='Todo el catálogo']")
        self.__logger.info("Navigated to categories")

        await self.__driver.wait_for_presence_css(".salt-m-t--0")

        soup = BeautifulSoup(await self.__driver.page_source(), "html.parser")
        category_tags = soup.select(".salt-m-t--0 li a")
        for tag in category_tags:
            name = tag.text
            if name in self.__skipped_categories:
                continue
            href = str(tag.get("href", "")).removeprefix("/")
            self.__category_urls[name] = f"{self.__url}/{href}"

        categories = list(self.__category_urls.keys())
        self.__logger.info("Found %d categories", len(categories))
        return categories

    @override
    async def get_subcategories(self, category: str) -> list[Subcategory]:
        category_url = self.__category_urls[category]
        await self.__driver.get(category_url)
        await self.__driver.wait_for_presence_css(".salt-m-t--0")

        soup = BeautifulSoup(await self.__driver.page_source(), "html.parser")
        subcategory_tags = soup.select(".salt-m-t--0 li a")
        subcategories = [
            Subcategory(
                name=tag.text,
                url=f"{self.__url}/{str(tag.get('href', '')).removeprefix('/')}",
            )
            for tag in subcategory_tags
        ]
        self.__logger.info(
            "Found %d subcategories in category '%s'",
            len(subcategories),
            category,
        )
        return subcategories

    @override
    async def scrape_subcategory(self, subcategory: Subcategory) -> list[Product]:
        await self.__try_close_popups()
        products = await self.__get_products(subcategory)
        self.__logger.info(
            "Scraped subcategory '%s' with %d products",
            subcategory.name,
            len(products),
        )
        return products

    async def __get_products(self, subcategory: Subcategory) -> list[Product]:
        await self.__driver.get(subcategory.url)

        await self.__driver.wait_for_presence_xpath("//h3[@data-test='fop-title']")

        products: list[Product] = []
        while True:
            soup = BeautifulSoup(await self.__driver.page_source(), "html.parser")

            product_tags = list(
                filter(
                    lambda tag: not tag.is_featured(),
                    map(AlcampoProductTag, soup.select("div.product-card-container")),
                )
            )
            last_added_product = products[-1] if products else None
            products += self.__get_new_products(product_tags, last_added_product)

            if len(products) >= len(product_tags):
                break
            try:
                await self.__driver.execute_script(
                    "window.scrollBy(0, window.innerHeight);"
                )
            except Exception:
                break

        return products

    def __get_new_products(
        self, product_tags: list[AlcampoProductTag], last_added_product: Product | None
    ) -> list[Product]:
        visible_products = [
            tag.to_product() for tag in product_tags if not tag.is_skeleton()
        ]

        index_of_last_added_product = (
            visible_products.index(last_added_product)
            if last_added_product is not None and last_added_product in visible_products
            else -1
        )
        return visible_products[index_of_last_added_product + 1 :]

    async def __try_close_popups(self) -> None:
        try:
            await self.__driver.wait_and_click_xpath(
                '//button[@data-test="popup-banner-close-button"]', timeout=1
            )
        except Exception:
            pass


class AlcampoProductTag:
    def __init__(self, tag: Tag) -> None:
        self.__tag = tag

    def to_product(self) -> Product:
        return Product(
            name=self.__name,
            quantity=self.__quantity,
            price=self.__price,
            image_url=self.__image_url,
        )

    def is_skeleton(self) -> bool:
        return self.__name == ""

    def is_featured(self) -> bool:
        return bool(self.__tag.select('span[data-test="fop-featured"]'))

    @property
    def __name(self) -> str:
        name_tag = self.__tag.select_one('h3[data-test="fop-title"]')
        return str(name_tag.text).strip() if name_tag else ""

    @property
    def __quantity(self) -> str:
        quantity_tag = self.__tag.select_one('div[data-test="fop-size"] span')
        return str(quantity_tag.text).strip() if quantity_tag else ""

    @property
    def __price(self) -> float:
        price_tag = self.__tag.select_one('span[data-test="fop-price"]')
        if not price_tag:
            return 0.0

        return float(
            re.sub(
                r"\s+|€",
                "",
                str(price_tag.text).replace(",", "."),
            )
        )

    @property
    def __image_url(self) -> str:
        image_tag = self.__tag.select_one('img[data-test="lazy-load-image"]')
        return str(image_tag.get("src", "")).strip() if image_tag else ""
