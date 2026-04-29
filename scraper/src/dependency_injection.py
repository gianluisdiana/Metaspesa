import grpc.aio

from application.abstractions import (
    FallbackProductRepository,
    MarketWebScraper,
    ProductRepository,
)
from application.product_processors import (
    BrandExtractor,
    BrandSimplifier,
    ProductProcessor,
    QuantityRedundancyRemover,
)
from application.use_case import (
    RetryFailedSavesCommandHandler,
    ScrapeMarketsCommandHandler,
)
from config import AppConfig
from infrastructure.grpc.grpc_product_repository import GrpcProductRepository
from infrastructure.local_storage import CsvProductRepository
from infrastructure.market_scrapers.market_web_scraper_factory import (
    MarketWebScraperFactory,
)
from infrastructure.playwright_driver import PlaywrightDriver
from infrastructure.secrets import LocalSecretVault
from infrastructure.telemetry.instrumented_playwright_driver import (
    InstrumentedPlaywrightDriver,
)
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


def __create_product_processor(settings: AppConfig) -> ProductProcessor:
    first_processor = QuantityRedundancyRemover()
    first_processor.next(BrandSimplifier(settings.processor.replacements)).next(
        BrandExtractor(settings.processor.known_brands)
    )
    return first_processor


def __create_main_repository(
    settings: AppConfig, channel: grpc.aio.Channel
) -> ProductRepository:
    vault = LocalSecretVault()
    username = vault.read_secret(settings.credentials.username_secret)
    password = vault.read_secret(settings.credentials.password_secret)

    return GrpcProductRepository(channel, username, password)


def __create_fallback_repository(settings: AppConfig) -> FallbackProductRepository:
    return CsvProductRepository(settings.fallback_persistence.folder_path)


def create_scrape_handler(
    settings: AppConfig, web_driver: WebDriver, channel: grpc.aio.Channel
) -> ScrapeMarketsCommandHandler:
    return ScrapeMarketsCommandHandler(
        main_repository=__create_main_repository(settings, channel),
        fallback_repository=__create_fallback_repository(settings),
        market_web_scrapers=__create_market_web_scrapers(settings, web_driver),
        product_processor=__create_product_processor(settings),
    )


def create_retry_handler(
    settings: AppConfig, channel: grpc.aio.Channel
) -> RetryFailedSavesCommandHandler:
    return RetryFailedSavesCommandHandler(
        fallback_repository=__create_fallback_repository(settings),
        main_repository=__create_main_repository(settings, channel),
    )


async def create_web_driver() -> WebDriver:
    raw_driver = await PlaywrightDriver.create(headless=True)
    web_driver = InstrumentedPlaywrightDriver(raw_driver)
    return web_driver
