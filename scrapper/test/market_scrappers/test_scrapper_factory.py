from config import AllScrapersSettings, AppConfig, ParsingSettings, ScraperSettings
from market_scrappers.alcampo_scrapper import AlcampoScrapper
from market_scrappers.mercadona_scrapper import MercadonaScrapper
from market_scrappers.scrapper_factory import ScrapperFactory


def test_raises_value_error_for_unsupported_market():
    # Arrange
    settings = __create_default_settings()
    factory = ScrapperFactory(settings)
    market = "UnsupportedMarket"

    # Act & Assert
    try:
        factory.create(market)
    except ValueError as e:
        assert str(e) == f"Unsupported market: {market}"


def test_creates_mercadona_scrapper():
    # Arrange
    settings = __create_default_settings()
    factory = ScrapperFactory(settings)
    market = "Mercadona"

    # Act
    scrapper = factory.create(market)

    # Assert
    assert isinstance(scrapper, MercadonaScrapper)


def test_creates_alcampo_scrapper():
    # Arrange
    settings = __create_default_settings()
    factory = ScrapperFactory(settings)
    market = "Alcampo"

    # Act
    scrapper = factory.create(market)

    # Assert
    assert isinstance(scrapper, AlcampoScrapper)


def __create_default_settings() -> AppConfig:
    return AppConfig(
        parsing=ParsingSettings(known_brands=[]),
        scrapers=AllScrapersSettings(alcampo=ScraperSettings(skipped_categories=[])),
    )
