import pytest
from selenium.webdriver.firefox.options import Options
from selenium.webdriver.firefox.webdriver import WebDriver

from config import AllScrapersSettings, AppConfig, ParsingSettings, ScraperSettings
from market_scrappers.scrapper_factory import ScrapperFactory


def test_raises_value_error_for_unsupported_market():
    # Arrange
    settings = __create_default_settings()
    factory = ScrapperFactory(settings)
    market = "UnsupportedMarket"
    web_driver = __create_web_driver()

    # Act & Assert
    try:
        factory.create(market, web_driver)
    except ValueError as e:
        assert str(e) == f"Unsupported market: {market}"


@pytest.mark.parametrize("market", ["Mercadona", "Alcampo"])
def test_does_not_raise_error_for_supported_market(market: str):
    # Arrange
    settings = __create_default_settings()
    factory = ScrapperFactory(settings)
    web_driver = __create_web_driver()

    # Act
    scrapper = factory.create(market, web_driver)

    # Assert
    assert scrapper is not None


def __create_default_settings() -> AppConfig:
    return AppConfig(
        parsing=ParsingSettings(known_brands=[]),
        scrapers=AllScrapersSettings(alcampo=ScraperSettings(skipped_categories=[])),
    )


def __create_web_driver():
    options = Options()
    options.add_argument("--headless")
    driver = WebDriver(options=options)
    return driver
