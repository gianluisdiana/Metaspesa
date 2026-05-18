import logging
from typing import ClassVar, override

from bs4 import BeautifulSoup, Tag

from application.abstractions import MarketWebScraper
from domain import Product, Subcategory
from infrastructure.market_scrapers.product_tags import MercadonaProductTag, ProductTag
from infrastructure.market_scrapers.product_window_scraper import ProductWindowScraper
from infrastructure.market_scrapers.resilience import RetryPolicy
from infrastructure.web_driver import Selector, WebDriver


class MercadonaWebScraper(MarketWebScraper):
    __selectors: ClassVar[dict[str, Selector]] = {
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
        self.__product_window_scraper = ProductWindowScraper(
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
        self.__logger.info(
            "Location set to postal code %s",
            postal_code,
            extra={"postal_code": postal_code},
        )

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

        self.__logger.info(
            "Found %d categories",
            len(categories),
            extra={"category_count": len(categories)},
        )
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
            extra={"subcategory_count": len(subcategories), "category_name": category},
        )
        return subcategories

    @override
    async def scrape_subcategory(self, subcategory: Subcategory) -> list[Product]:
        products = await self.__retry_policy.run(
            lambda: self.__scrape_subcategory(subcategory),
            description=f"Mercadona subcategory '{subcategory.name}'",
            logger=self.__logger,
            recover=lambda: self.__driver.wait(1),
        )
        return products or []

    async def __scrape_subcategory(self, subcategory: Subcategory) -> list[Product]:
        await self.__driver.get(subcategory.url)
        products = await self.__get_products()

        self.__logger.info(
            "Scraped subcategory '%s' with %d products",
            subcategory.name,
            len(products),
            extra={
                "subcategory_name": subcategory.name,
                "product_count": len(products),
            },
        )
        return products

    async def __get_products(self) -> list[Product]:
        await self.__driver.wait_for_presence(self.__selectors["subcategory_title"])

        return await self.__product_window_scraper.scrape(
            get_page_source=self.__driver.page_source,
            parse_tags=self.__parse_product_tags,
            scroll=lambda: self.__driver.execute_script(
                "window.scrollBy(0, window.innerHeight);"
            ),
            recover_after_failed_extraction=lambda: self.__driver.wait(1),
        )

    def __parse_product_tags(self, soup: BeautifulSoup) -> list[ProductTag]:
        return [
            MercadonaProductTag(tag)
            for tag in soup.select("button.product-cell__content-link")
        ]
