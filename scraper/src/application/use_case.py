import asyncio
import logging
from datetime import date

from application.abstractions import (
    FallbackProductRepository,
    MarketWebScraper,
    ProductRepository,
    RepositorySaveException,
)
from application.product_processors import ProductProcessor
from domain import Product


class RetryFailedSavesCommandHandler:
    def __init__(
        self,
        fallback_repository: FallbackProductRepository,
        main_repository: ProductRepository,
    ) -> None:
        self.__fallback_repository = fallback_repository
        self.__main_repository = main_repository
        self.__logger = logging.getLogger(self.__class__.__name__)

    async def handle(self) -> None:
        markets_and_dates = await self.__fallback_repository.get_markets_and_dates()
        for market_name, register_date in markets_and_dates:
            await self.__retry_saving_in_main_repository(market_name, register_date)

    async def __retry_saving_in_main_repository(
        self, market_name: str, date: date
    ) -> None:
        products = await self.__fallback_repository.get_products_by_market_and_date(
            market_name, date
        )
        try:
            await self.__main_repository.save(market_name, date, products)
            await self.__fallback_repository.remove_old_products(market_name, date)
            self.__logger.info(
                "Successfully retried saving products for market %s registered at %s",
                market_name,
                date.isoformat(),
            )
        except RepositorySaveException as ex:
            self.__logger.exception(
                "Failed to save products for market %s registered at %s",
                market_name,
                date.isoformat(),
                exc_info=ex,
            )


class ScrapeMarketsCommandHandler:
    def __init__(
        self,
        main_repository: ProductRepository,
        fallback_repository: FallbackProductRepository,
        market_web_scrapers: dict[str, MarketWebScraper],
        product_processor: ProductProcessor,
    ) -> None:
        self.__main_repository = main_repository
        self.__fallback_repository = fallback_repository
        self.__market_web_scrapers = market_web_scrapers
        self.__product_processor = product_processor

    async def handle(self, postal_code: str) -> None:
        assert len(self.__market_web_scrapers) > 0

        now = date.today()
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
        try:
            await self.__main_repository.save(market_name, date, products)
        except RepositorySaveException:
            await self.__fallback_repository.save(market_name, date, products)

    async def __scrape(
        self, scraper: MarketWebScraper, postal_code: str
    ) -> list[Product]:
        await scraper.set_location(postal_code)
        categories = await scraper.get_categories()
        products: list[Product] = []
        for category in categories:
            subcategories = await scraper.get_subcategories(category)
            for subcategory in subcategories:
                products += await scraper.scrape_subcategory(subcategory)
        return products
