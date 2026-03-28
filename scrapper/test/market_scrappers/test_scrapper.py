from logging import Logger
from typing import override

from domain import Product
from market_scrappers.market_web_scrapper import MarketWebScrapper
from market_scrappers.scrapper import Scrapper


class SpyMarketWebScrapper(MarketWebScrapper):
    def __init__(self, categories: list[str] | None = None):
        self.had_navigated_to_home = False
        self.had_closed_popups = False
        self.had_set_location = False
        self.set_location_called_with: str | None = None
        self.had_navigated_to_categories = False
        self.had_gotten_categories = False
        self.__categories = categories or []
        self.had_scraped_category = False

    @override
    def navigate_to_home(self) -> None:
        self.had_navigated_to_home = True

    @override
    def close_popups(self) -> None:
        self.had_closed_popups = True

    @override
    def set_location(self, postal_code: str) -> None:
        self.had_set_location = True
        self.set_location_called_with = postal_code

    @override
    def navigate_to_categories(self) -> None:
        self.had_navigated_to_categories = True

    @override
    def get_categories(self) -> list[str]:
        self.had_gotten_categories = True
        return self.__categories

    @override
    def scrape_category(self, category: str) -> list[Product]:
        self.had_scraped_category = True
        return []


class FakeMarketWebScrapper(SpyMarketWebScrapper):
    def __init__(self, products: list[Product]):
        self.__products = products

    @override
    def get_categories(self) -> list[str]:
        return [""]

    @override
    def scrape_category(self, category: str) -> list[Product]:
        return self.__products


def test_navigates_to_home():
    # Arrange
    market_web_scrapper = SpyMarketWebScrapper()

    scrapper = Scrapper(
        web_scrapper=market_web_scrapper,
        logger=Logger("test"),
    )

    # Act
    scrapper.scrape("12345")

    # Assert
    assert market_web_scrapper.had_navigated_to_home


def test_closes_popups():
    # Arrange
    market_web_scrapper = SpyMarketWebScrapper()

    scrapper = Scrapper(
        web_scrapper=market_web_scrapper,
        logger=Logger("test"),
    )

    # Act
    scrapper.scrape("12345")

    # Assert
    assert market_web_scrapper.had_closed_popups


def test_sets_location():
    # Arrange
    market_web_scrapper = SpyMarketWebScrapper()

    scrapper = Scrapper(
        web_scrapper=market_web_scrapper,
        logger=Logger("test"),
    )

    # Act
    scrapper.scrape("12345")

    # Assert
    assert market_web_scrapper.had_set_location


def test_sets_location_with_given_postal_code():
    # Arrange
    market_web_scrapper = SpyMarketWebScrapper()

    scrapper = Scrapper(
        web_scrapper=market_web_scrapper,
        logger=Logger("test"),
    )

    # Act
    scrapper.scrape("12345")

    # Assert
    assert market_web_scrapper.set_location_called_with == "12345"


def test_navigates_to_categories():
    # Arrange
    market_web_scrapper = SpyMarketWebScrapper()

    scrapper = Scrapper(
        web_scrapper=market_web_scrapper,
        logger=Logger("test"),
    )

    # Act
    scrapper.scrape("12345")

    # Assert
    assert market_web_scrapper.had_navigated_to_categories


def test_gets_categories():
    # Arrange
    market_web_scrapper = SpyMarketWebScrapper()

    scrapper = Scrapper(
        web_scrapper=market_web_scrapper,
        logger=Logger("test"),
    )

    # Act
    scrapper.scrape("12345")

    # Assert
    assert market_web_scrapper.had_gotten_categories


def test_does_not_scrape_categories_if_no_categories_found():
    # Arrange
    categories: list[str] = []
    market_web_scrapper = SpyMarketWebScrapper(categories)

    scrapper = Scrapper(
        web_scrapper=market_web_scrapper,
        logger=Logger("test"),
    )

    # Act
    scrapper.scrape("12345")

    # Assert
    assert not market_web_scrapper.had_scraped_category


def test_scrapes_categories_if_found():
    # Arrange
    categories: list[str] = ["category1", "category2"]
    market_web_scrapper = SpyMarketWebScrapper(categories)

    scrapper = Scrapper(
        web_scrapper=market_web_scrapper,
        logger=Logger("test"),
    )

    # Act
    scrapper.scrape("12345")

    # Assert
    assert market_web_scrapper.had_scraped_category


def test_returns_products_from_scraped_categories():
    scrapped_products: list[Product] = [
        Product(name="product1", price=1.0, quantity="1 unit"),
        Product(name="product2", price=2.0, quantity="1 unit"),
    ]
    market_web_scrapper = FakeMarketWebScrapper(scrapped_products)

    scrapper = Scrapper(
        web_scrapper=market_web_scrapper,
        logger=Logger("test"),
    )

    # Act
    products = scrapper.scrape("12345")

    # Assert
    assert products == scrapped_products
