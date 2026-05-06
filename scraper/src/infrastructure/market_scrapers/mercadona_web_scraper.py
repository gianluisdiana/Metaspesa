import logging
from typing import override

from bs4 import BeautifulSoup, Tag

from application.abstractions import MarketWebScraper
from domain import Product, Subcategory
from infrastructure.market_scrapers.resilience import (
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
        await self.__driver.wait_for_presence_css("h1.category-detail__title")
        soup = BeautifulSoup(await self.__driver.page_source(), "html.parser")

        product_tags = soup.select("button.product-cell__content-link")
        products = [MercadonaProductTag(tag).to_product() for tag in product_tags]

        self.__logger.info(
            "Scraped subcategory '%s' with %d products",
            subcategory.name,
            len(products),
        )
        return products


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

    @property
    def __name(self) -> str:
        return self.__required_text("h4.product-cell__description-name", "name")

    @property
    def __quantity(self) -> str:
        quantity_tag = self.__tag.select_one("div.product-format__size--cell")
        quantity = (
            str(quantity_tag.get("aria-label", "")).strip() if quantity_tag else ""
        )
        if quantity == "":
            raise MissingProductAttributeError("quantity")
        return quantity

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
        if image_url == "":
            raise MissingProductAttributeError("image_url")
        return image_url

    def __required_text(self, selector: str, attribute: str) -> str:
        tag = self.__tag.select_one(selector)
        text = str(tag.text).strip() if tag else ""
        if text == "":
            raise MissingProductAttributeError(attribute)
        return text
