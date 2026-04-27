import grpc.aio

from application.abstractions import MarketWebScraper, ProductRepository
from application.app_product_repository import AppProductRepository
from application.product_processors import (
    BrandExtractor,
    BrandSimplifier,
    ProductProcessor,
    QuantityRedundancyRemover,
)
from application.use_case import ScrapeMarketsCommandHandler
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


def __create_product_repository(
    settings: AppConfig, channel: grpc.aio.Channel
) -> ProductRepository:
    vault = LocalSecretVault()
    username = vault.read_secret(settings.credentials.username_secret)
    password = vault.read_secret(settings.credentials.password_secret)

    main_repository = GrpcProductRepository(channel, username, password)
    fallback_repository = CsvProductRepository(
        settings.fallback_persistence.folder_path
    )

    return AppProductRepository(
        main_repository=main_repository,
        fallback_repository=fallback_repository,
    )


def create_handler(
    settings: AppConfig, web_driver: WebDriver, channel: grpc.aio.Channel
) -> ScrapeMarketsCommandHandler:
    return ScrapeMarketsCommandHandler(
        product_repository=__create_product_repository(settings, channel),
        market_web_scrapers=__create_market_web_scrapers(settings, web_driver),
        product_processor=__create_product_processor(settings),
    )


async def create_web_driver() -> WebDriver:
    raw_driver = await PlaywrightDriver.create(headless=True)
    web_driver = InstrumentedPlaywrightDriver(raw_driver)
    return web_driver
