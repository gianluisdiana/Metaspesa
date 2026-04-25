import asyncio
import logging
import os
from pathlib import Path

from application.abstractions import MarketWebScraper
from application.product_processors import (
    BrandExtractor,
    BrandSimplifier,
    QuantityRedundancyRemover,
)
from application.use_case import ScrapeMarketsCommandHandler
from config import AppConfig, load_config
from infrastructure.local_storage import CsvProductRepository
from infrastructure.market_scrapers.market_web_scraper_factory import (
    MarketWebScraperFactory,
)
from infrastructure.playwright_driver import PlaywrightDriver
from infrastructure.telemetry.instrumented_playwright_driver import (
    InstrumentedPlaywrightDriver,
)
from infrastructure.telemetry.otel import setup_telemetry
from infrastructure.web_driver import WebDriver


def __create_market_web_scrapers(
    settings: AppConfig, web_driver: WebDriver
) -> dict[str, MarketWebScraper]:
    factory = MarketWebScraperFactory(settings, web_driver)
    market_names = [
        "Alcampo",
        "Mercadona",
    ]
    return {name: factory.create(name) for name in market_names}


def __create_product_processor(settings: AppConfig):
    first_processor = QuantityRedundancyRemover()
    first_processor.next(BrandSimplifier(settings.processor.replacements)).next(
        BrandExtractor(settings.processor.known_brands)
    )
    return first_processor


async def __create_web_driver():
    raw_driver = await PlaywrightDriver.create(headless=True)
    web_driver = InstrumentedPlaywrightDriver(raw_driver)
    return web_driver


async def main() -> None:
    logging.basicConfig(
        level=logging.INFO,
        format="\033[1m%(asctime)s\033[0m - \033[1m%(levelname)s\033[0m: %(name)s\n\t%(message)s",  # noqa: E501
    )

    telemetry = setup_telemetry(os.getenv("OTEL_EXPORTER_OTLP_ENDPOINT"))

    settings: AppConfig = load_config()
    web_driver = await __create_web_driver()
    product_repository = CsvProductRepository(Path("data"))
    scrapers: dict[str, MarketWebScraper] = __create_market_web_scrapers(
        settings, web_driver
    )
    product_processor = __create_product_processor(settings)

    handler = ScrapeMarketsCommandHandler(
        product_repository, scrapers, product_processor
    )

    with telemetry.measure_run():
        await handler.handle("38320")

    await web_driver.quit()


if __name__ == "__main__":
    asyncio.run(main())
