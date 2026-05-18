import logging
from contextlib import suppress
from typing import ClassVar, override

from bs4 import BeautifulSoup

from application.abstractions import MarketWebScraper
from config import ScraperSettings
from domain import Product, Subcategory
from infrastructure.market_scrapers.product_tags import AlcampoProductTag, ProductTag
from infrastructure.market_scrapers.product_window_scraper import ProductWindowScraper
from infrastructure.market_scrapers.resilience import (
    SCRAPER_RECOVERABLE_ERRORS,
    RetryPolicy,
)
from infrastructure.web_driver import Selector, WebDriver


class AlcampoWebScraper(MarketWebScraper):
    __selectors: ClassVar[dict[str, Selector]] = {
        "reject_cookies_button": Selector(target="#onetrust-reject-all-handler"),
        "close_offer_popup_button": Selector(
            target="button[data-test='popup-banner-close-button']"
        ),
        "location_menu_button": Selector(
            target="button[data-test='delivery-destination-selector-button']"
        ),
        "change_location_button": Selector(
            target="button[data-test='delivery-method-change-address-button']"
        ),
        "location_search_input": Selector(target="input[data-test='search-input']"),
        "dropdown_location": Selector(
            target="div[data-test='address-search-item-dropdown']"
        ),
        "confirm_location_button": Selector(
            target='//span[contains(text(), "Confirmar ubicaci")]',
            type="xpath",
        ),
        "confirm_delivery_method_button": Selector(
            target="button[data-test='choose-delivery-method-submit']"
        ),
        "menu_button": Selector(target=".dropdown-item-button"),
        "catalog_menu_button": Selector(
            target='//span[contains(text(), "Todo el cat")]',
            type="xpath",
        ),
        "categories_container": Selector(target=".salt-m-t--0"),
        "product_header": Selector(target="h3[data-test='fop-title']"),
    }

    def __init__(self, driver: WebDriver, settings: ScraperSettings) -> None:
        super().__init__()
        self.__driver = driver
        self.__skipped_categories: set[str] = set(settings.skipped_categories)
        self.__category_urls: dict[str, str] = {}
        self.__logger = logging.getLogger(self.__class__.__name__)
        self.__retry_policy = RetryPolicy()
        self.__product_window_scraper = ProductWindowScraper(
            self.__retry_policy, self.__logger
        )
        self.__url = "https://www.compraonline.alcampo.es"

    @override
    async def set_location(self, postal_code: str) -> None:
        await self.__driver.get(self.__url)

        await self.__driver.wait_and_click(self.__selectors["reject_cookies_button"])
        with suppress(*SCRAPER_RECOVERABLE_ERRORS):
            await self.__driver.wait_for_invisibility_css(
                self.__selectors["reject_cookies_button"].target
            )

        await self.__driver.wait_and_click(
            self.__selectors["close_offer_popup_button"], timeout=10
        )
        await self.__driver.wait_and_click(self.__selectors["location_menu_button"])
        await self.__driver.wait_and_click(self.__selectors["change_location_button"])

        await self.__driver.wait_for_presence(self.__selectors["location_search_input"])
        await self.__driver.wait_and_send_keys(
            self.__selectors["location_search_input"], postal_code
        )

        await self.__driver.wait_and_click(self.__selectors["dropdown_location"])
        await self.__driver.wait_and_click(self.__selectors["confirm_location_button"])
        await self.__driver.wait_and_click(
            self.__selectors["confirm_delivery_method_button"]
        )

        await self.__driver.refresh()
        self.__logger.info(
            "Location set to postal code %s",
            postal_code,
            extra={"postal_code": postal_code},
        )

    @override
    async def get_categories(self) -> list[str]:
        categories = await self.__retry_policy.run(
            self.__get_categories,
            description="Alcampo category discovery",
            logger=self.__logger,
        )
        return categories or []

    async def __get_categories(self) -> list[str]:
        await self.__driver.wait_and_click(self.__selectors["menu_button"])
        await self.__driver.wait_and_click(self.__selectors["catalog_menu_button"])
        self.__logger.info("Navigated to categories")

        await self.__driver.wait_for_presence(self.__selectors["categories_container"])
        soup = BeautifulSoup(await self.__driver.page_source(), "html.parser")

        category_tags = soup.select(".salt-m-t--0 li a")
        for tag in category_tags:
            name = tag.text
            if name in self.__skipped_categories:
                continue
            href = str(tag.get("href", "")).removeprefix("/")
            self.__category_urls[name] = f"{self.__url}/{href}"

        categories = list(self.__category_urls.keys())
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
            description=f"Alcampo category '{category}'",
            logger=self.__logger,
        )
        return subcategories or []

    async def __get_subcategories(self, category: str) -> list[Subcategory]:
        category_url = self.__category_urls.get(category)
        if not category_url:
            self.__logger.warning(
                "Skipping unknown category '%s'",
                category,
                extra={"category_name": category},
            )
            return []

        await self.__driver.get(category_url)
        await self.__driver.wait_for_presence(self.__selectors["categories_container"])
        soup = BeautifulSoup(await self.__driver.page_source(), "html.parser")

        subcategory_tags = soup.select(
            f"{self.__selectors['categories_container'].target} li a"
        )
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
            extra={"subcategory_count": len(subcategories), "category_name": category},
        )
        return subcategories

    @override
    async def scrape_subcategory(self, subcategory: Subcategory) -> list[Product]:
        await self.__driver.get(subcategory.url)

        products = await self.__retry_policy.run(
            lambda: self.__scrape_subcategory(subcategory),
            description=f"Alcampo subcategory '{subcategory.name}'",
            logger=self.__logger,
            recover=lambda: self.__driver.get(subcategory.url),
        )
        return products or []

    async def __scrape_subcategory(self, subcategory: Subcategory) -> list[Product]:
        await self.__try_close_popups()
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
        await self.__driver.wait_for_presence(self.__selectors["product_header"])

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
            tag
            for tag in map(AlcampoProductTag, soup.select("div.product-card-container"))
            if not tag.is_featured()
        ]

    async def __try_close_popups(self) -> None:
        with suppress(*SCRAPER_RECOVERABLE_ERRORS):
            await self.__driver.wait_and_click(
                self.__selectors["close_offer_popup_button"], timeout=1
            )
