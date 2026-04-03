import logging
from pathlib import Path

from selenium.webdriver.firefox.options import Options
from selenium.webdriver.firefox.webdriver import WebDriver

from application.abstractions import MarketWebScrapper
from application.product_processors import (
    BrandExtractor,
    BrandSimplifier,
    QuantityRedundancyRemover,
)
from application.use_case import ScrapeMarketsCommandHandler
from config import AppConfig, load_config
from infrastructure.local_storage import CsvProductRepository
from infrastructure.market_scrappers.market_web_scrapper_factory import (
    MarketWebScrapperFactory,
)


def __create_web_driver() -> WebDriver:
    options = Options()
    options.add_argument("--headless")
    driver = WebDriver(options=options)
    return driver


def __create_market_web_scrappers(
    settings: AppConfig, web_driver: WebDriver
) -> dict[str, MarketWebScrapper]:
    factory = MarketWebScrapperFactory(settings, web_driver)
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


def main() -> None:
    logging.basicConfig(
        level=logging.INFO,
        format="\033[1m%(asctime)s\033[0m - \033[1m%(levelname)s\033[0m: %(name)s\n\t%(message)s",  # noqa: E501
    )

    settings: AppConfig = load_config()
    web_driver: WebDriver = __create_web_driver()
    product_repository = CsvProductRepository(Path("data"))
    scrappers: dict[str, MarketWebScrapper] = __create_market_web_scrappers(
        settings, web_driver
    )
    product_processor = __create_product_processor(settings)

    handler = ScrapeMarketsCommandHandler(
        product_repository, scrappers, product_processor
    )
    handler.handle("38320")

    web_driver.quit()


if __name__ == "__main__":
    main()
