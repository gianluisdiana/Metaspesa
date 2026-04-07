from typing import Any

from config import AppConfig, ProcessorSettings, ScraperSettings
from infrastructure.market_scrappers.alcampo_web_scrapper import AlcampoWebScrapper
from infrastructure.market_scrappers.market_web_scrapper_factory import (
    MarketWebScrapperFactory,
)
from infrastructure.market_scrappers.mercadona_web_scrapper import MercadonaWebScrapper


def test_raises_value_error_for_unsupported_market():
    # Arrange
    settings = __create_default_settings()
    web_driver = __create_web_driver()
    factory = MarketWebScrapperFactory(settings, web_driver)
    market = "UnsupportedMarket"

    # Act & Assert
    try:
        factory.create(market)
    except ValueError as e:
        assert str(e) == f"Unsupported market: {market}"


def test_creates_mercadona_web_scrapper():
    # Arrange
    settings = __create_default_settings()
    web_driver = __create_web_driver()
    factory = MarketWebScrapperFactory(settings, web_driver)

    # Act
    web_scrapper = factory.create("Mercadona")

    # Assert
    assert isinstance(web_scrapper, MercadonaWebScrapper)


def test_creates_alcampo_web_scrapper():
    # Arrange
    settings = __create_default_settings()
    web_driver = __create_web_driver()
    factory = MarketWebScrapperFactory(settings, web_driver)

    # Act
    web_scrapper = factory.create("Alcampo")

    # Assert
    assert isinstance(web_scrapper, AlcampoWebScrapper)


def __create_default_settings() -> AppConfig:
    return AppConfig(
        processor=ProcessorSettings(known_brands=[], replacements={}),
        scrapers=ScraperSettings(skipped_categories=[]),
    )


def __create_web_driver() -> Any:
    class _StubDriver:
        pass

    return _StubDriver()
