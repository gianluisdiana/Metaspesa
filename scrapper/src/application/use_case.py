import logging

from application.abstractions import MarketWebScrapper, ProductRepository
from domain import Product


class ScrapeMarketsCommandHandler:
    def __init__(
        self,
        product_repository: ProductRepository,
        market_web_scrappers: dict[str, MarketWebScrapper],
    ) -> None:
        self.__product_repository = product_repository
        self.__market_web_scrappers = market_web_scrappers

    def handle(self, postal_code: str) -> None:
        assert len(self.__market_web_scrappers) > 0

        for market_name in self.__market_web_scrappers:
            products: list[Product] = self.__scrape_market(market_name, postal_code)
            self.__product_repository.save(market_name, products)

    def __scrape_market(self, market_name: str, postal_code: str) -> list[Product]:
        logger = logging.getLogger(f"{market_name}WebScrapper")
        market_web_scrapper = self.__market_web_scrappers[market_name]

        market_web_scrapper.navigate_to_home()
        logger.info(f"Navigated to {market_web_scrapper.url}")

        market_web_scrapper.close_popups()
        logger.info("Closed popups")

        market_web_scrapper.set_location(postal_code)
        logger.info(f"Location set to postal code {postal_code}")

        market_web_scrapper.navigate_to_categories()
        logger.info("Navigated to categories")

        categories = market_web_scrapper.get_categories()
        logger.info(f"Found {len(categories)} categories")

        products: list[Product] = []
        for category in categories:
            category_products = market_web_scrapper.scrape_category(category)
            products += category_products
            logger.info(
                f"Scraped category '{category}' with {len(category_products)} products"
            )

        return products
