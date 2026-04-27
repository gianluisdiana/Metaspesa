from typing import Any

from config import (
    AppConfig,
    CredentialsSettings,
    FallbackPersistenceSettings,
    ProcessorSettings,
    ScraperSettings,
)
from infrastructure.market_scrapers.alcampo_web_scraper import AlcampoWebScraper
from infrastructure.market_scrapers.market_web_scraper_factory import (
    MarketWebScraperFactory,
)
from infrastructure.market_scrapers.mercadona_web_scraper import MercadonaWebScraper


def test_raises_value_error_for_unsupported_market():
    # Arrange
    settings = __create_default_settings()
    web_driver = __create_web_driver()
    factory = MarketWebScraperFactory(settings, web_driver)
    market = "UnsupportedMarket"

    # Act & Assert
    try:
        factory.create(market)
    except ValueError as e:
        assert str(e) == f"Unsupported market: {market}"


def test_creates_mercadona_web_scraper():
    # Arrange
    settings = __create_default_settings()
    web_driver = __create_web_driver()
    factory = MarketWebScraperFactory(settings, web_driver)

    # Act
    web_scraper = factory.create("Mercadona")

    # Assert
    assert isinstance(web_scraper, MercadonaWebScraper)


def test_creates_alcampo_web_scraper():
    # Arrange
    settings = __create_default_settings()
    web_driver = __create_web_driver()
    factory = MarketWebScraperFactory(settings, web_driver)

    # Act
    web_scraper = factory.create("Alcampo")

    # Assert
    assert isinstance(web_scraper, AlcampoWebScraper)


def __create_default_settings() -> AppConfig:
    return AppConfig(
        processor=ProcessorSettings(known_brands=[], replacements={}),
        scrapers=ScraperSettings(skipped_categories=[]),
        fallback_persistence=FallbackPersistenceSettings(folder_path="data"),
        credentials=CredentialsSettings(
            username_secret="scraper_username",
            password_secret="scraper_password",
        ),
    )


def __create_web_driver() -> Any:
    class _StubDriver:
        pass

    return _StubDriver()
