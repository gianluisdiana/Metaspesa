from domain import Market
from market_scrappers.alcampo_scrapper import AlcampoScrapper
from market_scrappers.mercadona_scrapper import MercadonaScrapper
from market_scrappers.scrapper_factory import ScrapperFactory


def test_raises_value_error_for_unsupported_market():
    # Arrange
    factory = ScrapperFactory()
    market = Market(name="UnsupportedMarket", url="http://unsupportedmarket.com")

    # Act & Assert
    try:
        factory.create(market)
    except ValueError as e:
        assert str(e) == f"Unsupported market: {market.name}"


def test_creates_mercadona_scrapper():
    # Arrange
    factory = ScrapperFactory()
    market = Market(name="Mercadona", url="http://mercadona.com")

    # Act
    scrapper = factory.create(market)

    # Assert
    assert isinstance(scrapper, MercadonaScrapper)


def test_creates_alcampo_scrapper():
    # Arrange
    factory = ScrapperFactory()
    market = Market(name="Alcampo", url="http://alcampo.com")

    # Act
    scrapper = factory.create(market)

    # Assert
    assert isinstance(scrapper, AlcampoScrapper)
