from __future__ import annotations

from logging import Logger

from domain import Product
from market_scrappers.market_web_scrapper import MarketWebScrapper


class Scrapper:
    def __init__(self, web_scrapper: MarketWebScrapper, logger: Logger):
        self.__web_scrapper = web_scrapper
        self.__logger = logger

    def scrape(self, postal_code: str) -> list[Product]:
        self.__web_scrapper.navigate_to_home()
        self.__logger.info(f"Navigated to {self.__web_scrapper.url}")

        self.__web_scrapper.close_popups()
        self.__logger.info("Closed popups")

        self.__web_scrapper.set_location(postal_code)
        self.__logger.info(f"Location set to postal code {postal_code}")

        self.__web_scrapper.navigate_to_categories()
        self.__logger.info("Navigated to categories")

        categories = self.__web_scrapper.get_categories()
        self.__logger.info(f"Found {len(categories)} categories")

        products: list[Product] = []
        for category in categories:
            category_products = self.__web_scrapper.scrape_category(category)
            products += category_products
            self.__logger.info(
                f"Scraped category '{category}' with {len(category_products)} products"
            )

        return products
