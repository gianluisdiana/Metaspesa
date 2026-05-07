import logging
from typing import override
from urllib.parse import urlparse

from bs4 import BeautifulSoup, Tag

from application.abstractions import MarketWebScraper
from domain import Product, Subcategory
from infrastructure.market_scrapers.resilience import (
    SCRAPER_RECOVERABLE_ERRORS,
    MissingProductAttributeError,
    RetryPolicy,
)
from infrastructure.web_driver import WebDriver


class MercadonaWebScraper(MarketWebScraper):
    def __init__(self, driver: WebDriver) -> None:
        super().__init__()
        self.__driver = driver
        self.__logger = logging.getLogger(self.__class__.__name__)
        self.__retry_policy = RetryPolicy()
        self.__url = "https://tienda.mercadona.es"

    @override
    async def set_location(self, postal_code: str) -> None:
        await self.__driver.get(self.__url)

        await self.__driver.wait_and_click_css("button.ui-button:nth-child(3)")

        await self.__driver.wait_for_presence_xpath(
            '//input[@data-testid="postal-code-checker-input"]'
        )

        await self.__driver.wait_and_send_keys_xpath(
            '//input[@data-testid="postal-code-checker-input"]', postal_code
        )

        await self.__driver.wait_and_click_xpath(
            '//button[@data-testid="postal-code-checker-button"]'
        )
        self.__logger.info("Location set to postal code %s", postal_code)

    @override
    async def get_categories(self) -> list[str]:
        categories = await self.__retry_policy.run(
            self.__get_categories,
            description="Mercadona category discovery",
            logger=self.__logger,
        )
        return categories or []

    async def __get_categories(self) -> list[str]:
        await self.__driver.wait_for_presence_xpath('//a[contains(text(), "Categor")]')
        await self.__driver.wait_and_click_xpath('//a[contains(text(), "Categor")]')

        await self.__driver.wait_for_presence_css("li.category-menu__item")
        soup = BeautifulSoup(await self.__driver.page_source(), "html.parser")

        product_category_tags = soup.find_all("li", class_="category-menu__item")
        categories: list[str] = [
            (product_category_tag.select_one("label.subhead1-r") or Tag()).text
            for product_category_tag in product_category_tags
        ]

        self.__logger.info("Found %d categories", len(categories))
        return categories

    @override
    async def get_subcategories(self, category: str) -> list[Subcategory]:
        subcategories = await self.__retry_policy.run(
            lambda: self.__get_subcategories(category),
            description=f"Mercadona category '{category}'",
            logger=self.__logger,
        )
        return subcategories or []

    async def __get_subcategories(self, category: str) -> list[Subcategory]:
        await self.__driver.wait_and_click_xpath(
            f'//label[contains(@class, "subhead1-r") and text()="{category}"]',
        )

        await self.__driver.wait_for_presence_css(
            "li.category-menu__item div ul li button.category-item__link"
        )
        soup = BeautifulSoup(await self.__driver.page_source(), "html.parser")

        subcategory_tags = soup.select(
            "li.category-menu__item div ul li button.category-item__link"
        )
        subcategories = [
            Subcategory(
                name=tag.text,
                url=f"{self.__url}/categories/{tag.get('id', '')}",
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
        products = await self.__retry_policy.run(
            lambda: self.__scrape_subcategory(subcategory),
            description=f"Mercadona subcategory '{subcategory.name}'",
            logger=self.__logger,
            recover=lambda: self.__driver.execute_script("setTimeout(() => {}, 1000);"),
        )
        return products or []

    async def __scrape_subcategory(self, subcategory: Subcategory) -> list[Product]:
        await self.__driver.get(subcategory.url)
        products = await self.__get_products()

        self.__logger.info(
            "Scraped subcategory '%s' with %d products",
            subcategory.name,
            len(products),
        )
        return products

    async def __get_products(self) -> list[Product]:
        await self.__driver.wait_for_presence_css("h1.category-detail__title")

        products: list[Product] = []
        while True:
            products, product_tags = await self.__retry_policy.run(
                lambda ps=products: self.__get_products_from_current_window(ps),
                description="Extracting products from page",
                logger=self.__logger,
                recover=lambda: self.__driver.execute_script(
                    "setTimeout(() => {}, 1000);"
                ),
            ) or ([], [])

            if len(products) >= len(product_tags):
                break

            try:
                await self.__driver.execute_script(
                    "window.scrollBy(0, window.innerHeight);"
                )
            except SCRAPER_RECOVERABLE_ERRORS:
                break

        return products

    async def __get_products_from_current_window(
        self, products: list[Product]
    ) -> tuple[list[Product], list[MercadonaProductTag]]:
        soup = BeautifulSoup(await self.__driver.page_source(), "html.parser")

        product_tags = [
            MercadonaProductTag(tag)
            for tag in soup.select("button.product-cell__content-link")
        ]
        last_added_product = products[-1] if products else None
        products += self.__get_new_products(product_tags, last_added_product)
        return products, product_tags

    def __get_new_products(
        self,
        product_tags: list[MercadonaProductTag],
        last_added_product: Product | None,
    ) -> list[Product]:
        visible_products = [tag.to_product() for tag in product_tags if tag.is_ready()]
        index_of_last_added_product = (
            visible_products.index(last_added_product)
            if last_added_product is not None and last_added_product in visible_products
            else -1
        )
        return visible_products[index_of_last_added_product + 1 :]


class MercadonaProductTag:
    def __init__(self, tag: Tag) -> None:
        self.__tag = tag

    def to_product(self) -> Product:
        return Product(
            name=self.__name,
            quantity=self.__quantity,
            price=self.__price,
            image_url=self.__image_url,
        )

    def is_ready(self) -> bool:
        return (
            self.__text("h4.product-cell__description-name") != ""
            and self.__quantity_text != ""
            and self.__text("p.product-price__unit-price") != ""
            and self.__has_valid_image_url()
        )

    @property
    def __name(self) -> str:
        return self.__required_text("h4.product-cell__description-name", "name")

    @property
    def __quantity(self) -> str:
        quantity = self.__quantity_text
        if quantity == "":
            raise MissingProductAttributeError("quantity")
        return quantity

    @property
    def __quantity_text(self) -> str:
        quantity_tag = self.__tag.select_one("div.product-format__size--cell")
        if quantity_tag:
            texts = [text for text in quantity_tag.stripped_strings if text]
            if len(texts) > 1:
                return texts[1].strip()
            elif len(texts) == 1:
                return texts[0].strip()
        return ""

    @property
    def __price(self) -> float:
        price = self.__required_text("p.product-price__unit-price", "price")
        price = price.replace("€", "").replace(",", ".").strip()
        if price == "":
            raise MissingProductAttributeError("price")

        try:
            return float(price)
        except ValueError as ex:
            raise MissingProductAttributeError("price") from ex

    @property
    def __image_url(self) -> str:
        image_tag = self.__tag.select_one("div.product-cell__image-wrapper img")
        image_url = str(image_tag.get("src", "")).strip() if image_tag else ""
        if not self.__is_valid_url(image_url):
            raise MissingProductAttributeError("image_url")
        return image_url

    def __required_text(self, selector: str, attribute: str) -> str:
        text = self.__text(selector)
        if text == "":
            raise MissingProductAttributeError(attribute)
        return text

    def __text(self, selector: str) -> str:
        tag = self.__tag.select_one(selector)
        return str(tag.text).strip() if tag else ""

    def __has_valid_image_url(self) -> bool:
        image_tag = self.__tag.select_one("div.product-cell__image-wrapper img")
        image_url = str(image_tag.get("src", "")).strip() if image_tag else ""
        return self.__is_valid_url(image_url)

    @staticmethod
    def __is_valid_url(image_url: str) -> bool:
        parsed_url = urlparse(image_url)
        return parsed_url.scheme in {"http", "https"} and parsed_url.netloc != ""
