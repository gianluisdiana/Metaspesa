import asyncio
import logging
from datetime import date

from application.abstractions import (
    FallbackProductRepository,
    MarketWebScraper,
    ProductRepository,
    RepositorySaveException,
)
from application.clock import Clock, SystemClock
from application.product_processors import ProductProcessor
from domain import Product


class MissingMarketWebScrapersError(ValueError):
    def __init__(self) -> None:
        super().__init__("At least one market web scraper must be configured.")


class ScrapeMarketsCommandHandler:
    def __init__(
        self,
        main_repository: ProductRepository,
        fallback_repository: FallbackProductRepository,
        market_web_scrapers: dict[str, MarketWebScraper],
        product_processor: ProductProcessor,
        clock: Clock | None = None,
    ) -> None:
        self.__main_repository = main_repository
        self.__fallback_repository = fallback_repository
        self.__market_web_scrapers = market_web_scrapers
        self.__product_processor = product_processor
        self.__clock = clock or SystemClock()
        self.__logger = logging.getLogger(self.__class__.__name__)

    async def handle(self, postal_code: str) -> None:
        if len(self.__market_web_scrapers) == 0:
            raise MissingMarketWebScrapersError()

        now = self.__clock.today()
        saving_tasks: list[asyncio.Task[None]] = []
        for market_name in self.__market_web_scrapers:
            scraper = self.__market_web_scrapers[market_name]
            products = await self.__scrape(scraper, postal_code)
            products = [self.__product_processor.process(p) for p in products]
            saving_tasks.append(
                asyncio.create_task(self.__save_products(market_name, now, products))
            )

        await asyncio.gather(*saving_tasks)

    async def __save_products(
        self, market_name: str, date: date, products: list[Product]
    ) -> None:
        unique_products = self.__remove_repeated_products(market_name, products)
        try:
            await self.__main_repository.save(market_name, date, unique_products)
        except RepositorySaveException:
            await self.__fallback_repository.save(market_name, date, unique_products)

    def __remove_repeated_products(
        self, market_name: str, products: list[Product]
    ) -> list[Product]:
        unique_products: list[Product] = []
        for product in products:
            if product in unique_products:
                self.__logger.warning(
                    "Skipping repeated product for market %s: %s",
                    market_name,
                    product.name,
                    extra={"market_name": market_name, "product": product.name},
                )
                continue

            unique_products.append(product)

        return unique_products

    async def __scrape(
        self, scraper: MarketWebScraper, postal_code: str
    ) -> list[Product]:
        await scraper.set_location(postal_code)
        categories = await scraper.get_categories()
        products: list[Product] = []
        for category in categories[:1]:
            subcategories = await scraper.get_subcategories(category)
            for subcategory in subcategories[:1]:
                products += await scraper.scrape_subcategory(subcategory)
        return products
