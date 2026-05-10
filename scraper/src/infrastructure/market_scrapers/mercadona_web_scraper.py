import logging
from typing import override
from urllib.parse import urlparse

from bs4 import BeautifulSoup, Tag

from application.abstractions import MarketWebScraper
from domain import Product, Subcategory
from infrastructure.market_scrapers.product_scroll import (
    ProductScrollScraper,
)
from infrastructure.market_scrapers.resilience import (
    MissingProductAttributeError,
    RetryPolicy,
)
from infrastructure.web_driver import Selector, WebDriver


class MercadonaWebScraper(MarketWebScraper):
    __selectors: dict[str, Selector] = {
        "close_cookies_button": Selector(target="button.ui-button:nth-child(3)"),
        "postal_code_input": Selector(
            target='input[data-testid="postal-code-checker-input"]'
        ),
        "confirm_location_button": Selector(
            target='button[data-testid="postal-code-checker-button"]'
        ),
        "categories_page_link": Selector(
            target='//a[contains(text(), "Categor")]', type="xpath"
        ),
        "category_list_item": Selector(target="li.category-menu__item"),
        "subcategory_list_item": Selector(
            target="li.category-menu__item div ul li button.category-item__link"
        ),
        "subcategory_title": Selector(target="h1.category-detail__title"),
    }

    def __init__(self, driver: WebDriver) -> None:
        super().__init__()
        self.__driver = driver
        self.__logger = logging.getLogger(self.__class__.__name__)
        self.__retry_policy = RetryPolicy()
        self.__product_scroll_scraper = ProductScrollScraper(
            self.__retry_policy, self.__logger
        )
        self.__url = "https://tienda.mercadona.es"

    @override
    async def set_location(self, postal_code: str) -> None:
        await self.__driver.get(self.__url)

        await self.__driver.wait_and_click(self.__selectors["close_cookies_button"])

        await self.__driver.wait_for_presence(self.__selectors["postal_code_input"])

        await self.__driver.wait_and_send_keys(
            self.__selectors["postal_code_input"], postal_code
        )

        await self.__driver.wait_and_click(self.__selectors["confirm_location_button"])
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
        await self.__driver.wait_for_presence(self.__selectors["categories_page_link"])
        await self.__driver.wait_and_click(self.__selectors["categories_page_link"])

        await self.__driver.wait_for_presence(self.__selectors["category_list_item"])
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
        subcategory_selector = Selector(
            target=f'//label[contains(@class, "subhead1-r") and text()="{category}"]',
            type="xpath",
        )
        await self.__driver.wait_and_click(subcategory_selector)

        await self.__driver.wait_for_presence(self.__selectors["subcategory_list_item"])
        soup = BeautifulSoup(await self.__driver.page_source(), "html.parser")

        subcategory_tags = soup.select(self.__selectors["subcategory_list_item"].target)
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
        await self.__driver.wait_for_presence(self.__selectors["subcategory_title"])

        return await self.__product_scroll_scraper.scrape(
            get_products_from_current_window=self.__get_products_from_current_window,
            scroll=lambda: self.__driver.execute_script(
                "window.scrollBy(0, window.innerHeight);"
            ),
            recover_after_failed_extraction=lambda: self.__driver.execute_script(
                "setTimeout(() => {}, 1000);"
            ),
        )

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
