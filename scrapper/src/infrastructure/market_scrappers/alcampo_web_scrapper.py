import re
from typing import override

from bs4 import BeautifulSoup, Tag

from application.abstractions import MarketWebScrapper
from config import ScraperSettings
from domain import Product, Subcategory
from infrastructure.playwright_driver import PlaywrightDriver


class AlcampoWebScrapper(MarketWebScrapper):
    def __init__(self, driver: PlaywrightDriver, settings: ScraperSettings) -> None:
        super().__init__()
        self.__driver = driver
        self.__skipped_categories: set[str] = set(settings.skipped_categories)
        self.__category_urls: dict[str, str] = {}

    @property
    @override
    def url(self) -> str:
        return "https://www.compraonline.alcampo.es"

    @override
    def navigate_to_home(self) -> None:
        self.__driver.get(self.url)

    @override
    def close_popups(self) -> None:
        self.__driver.wait_and_click_css("#onetrust-reject-all-handler")
        try:
            self.__driver.wait_for_invisibility_css("#onetrust-reject-all-handler")
        except Exception:
            pass

        self.__driver.wait_and_click_xpath(
            '//button[@data-test="popup-banner-close-button"]', timeout=10
        )

    @override
    def set_location(self, postal_code: str) -> None:
        self.__driver.wait_and_click_css("._button--fill_ftyis_140")
        self.__driver.wait_and_click_css("._button--primary_ftyis_42 > span")

        self.__driver.wait_for_presence_xpath('//input[@data-test="search-input"]')
        self.__driver.wait_and_send_keys_xpath(
            '//input[@data-test="search-input"]', postal_code
        )

        self.__driver.wait_and_click_xpath(f'//span[contains(text(), "{postal_code}")]')
        self.__driver.wait_and_click_xpath('//span[text()="Confirmar ubicación"]')
        self.__driver.wait_and_click_xpath(
            '//button[@data-test="choose-delivery-method-submit"]'
        )

        self.__driver.refresh()

    @override
    def navigate_to_categories(self) -> None:
        self.__driver.wait_and_click_css(".dropdown-item-button")
        self.__driver.wait_and_click_xpath("//span[text()='Todo el catálogo']")

    @override
    def get_categories(self) -> list[str]:
        self.__driver.wait_for_presence_css(".salt-m-t--0")

        soup = BeautifulSoup(self.__driver.page_source, "html.parser")
        category_tags = soup.select(".salt-m-t--0 li a")
        for tag in category_tags:
            name = tag.text
            if name in self.__skipped_categories:
                continue
            href = str(tag.get("href", "")).removeprefix("/")
            self.__category_urls[name] = f"{self.url}/{href}"

        return list(self.__category_urls.keys())

    @override
    def scrape_category(self, category: str) -> list[Product]:
        category_url = self.__category_urls[category]
        category_scrapper = AlcampoCategoryScrapper(
            self.__driver, category_url, self.url
        )
        return category_scrapper.scrape()


class AlcampoCategoryScrapper:
    def __init__(self, driver: PlaywrightDriver, url: str, base_url: str) -> None:
        self.__driver = driver
        self.__url = url
        self.__base_url = base_url

    def scrape(self) -> list[Product]:
        self.__driver.get(self.__url)

        subcategories = self.__get_subcategories()

        products: list[Product] = []
        for subcategory in subcategories:
            self.__try_close_popups()
            products += self.__get_products(subcategory)

        return products

    def __get_subcategories(self) -> list[Subcategory]:
        self.__driver.wait_for_presence_css(".salt-m-t--0")

        soup = BeautifulSoup(self.__driver.page_source, "html.parser")
        subcategory_tags = soup.select(".salt-m-t--0 li a")
        return [
            Subcategory(
                name=tag.text,
                url=f"{self.__base_url}/{str(tag.get('href', '')).removeprefix('/')}",
            )
            for tag in subcategory_tags
        ]

    def __get_products(self, subcategory: Subcategory) -> list[Product]:
        self.__driver.get(subcategory.url)

        self.__driver.wait_for_presence_xpath("//h3[@data-test='fop-title']")

        products: list[Product] = []
        while True:
            soup = BeautifulSoup(self.__driver.page_source, "html.parser")

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
                self.__driver.execute_script("window.scrollBy(0, window.innerHeight);")
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

    def __try_close_popups(self) -> None:
        try:
            self.__driver.wait_and_click_xpath(
                '//button[@data-test="popup-banner-close-button"]', timeout=1
            )
        except Exception:
            pass


class AlcampoProductTag:
    def __init__(self, tag: Tag) -> None:
        self.__tag = tag

    def to_product(self) -> Product:
        return Product(name=self.__name, quantity=self.__quantity, price=self.__price)

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
