import logging

from application.abstractions import MarketWebScraper, ProductRepository
from application.product_processors import ProductProcessor
from domain import Product, Subcategory


class ScrapeMarketsCommandHandler:
    def __init__(
        self,
        product_repository: ProductRepository,
        market_web_scrapers: dict[str, MarketWebScraper],
        product_processor: ProductProcessor,
    ) -> None:
        self.__product_repository = product_repository
        self.__market_web_scrapers = market_web_scrapers
        self.__product_processor = product_processor

    async def handle(self, postal_code: str) -> None:
        assert len(self.__market_web_scrapers) > 0

        for market_name in self.__market_web_scrapers:
            products: list[Product] = await self.__scrape_market(
                market_name, postal_code
            )
            products = [
                self.__product_processor.process(product) for product in products
            ]
            await self.__product_repository.save(market_name, products)

    async def __scrape_market(
        self, market_name: str, postal_code: str
    ) -> list[Product]:
        logger = logging.getLogger(f"{market_name}WebScraper")
        market_web_scraper = self.__market_web_scrapers[market_name]

        await market_web_scraper.navigate_to_home()
        logger.info(f"Navigated to {market_web_scraper.url}")

        await market_web_scraper.close_popups()
        logger.info("Closed popups")

        await market_web_scraper.set_location(postal_code)
        logger.info(f"Location set to postal code {postal_code}")

        await market_web_scraper.navigate_to_categories()
        logger.info("Navigated to categories")

        categories = await market_web_scraper.get_categories()
        logger.info(f"Found {len(categories)} categories")

        products: list[Product] = []
        for category in categories:
            subcategories: list[
                Subcategory
            ] = await market_web_scraper.get_subcategories(category)
            logger.info(
                f"Found {len(subcategories)} subcategories in category '{category}'"
            )

            for subcategory in subcategories:
                subcategory_products = await market_web_scraper.scrape_subcategory(
                    subcategory
                )
                products += subcategory_products
                logger.info(
                    f"Scraped subcategory '{subcategory.name}' "
                    f"with {len(subcategory_products)} products"
                )

        return products
