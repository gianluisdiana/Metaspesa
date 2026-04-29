from typing import override

from conftest import (
    DummyFallbackRepository,
    DummyProductRepository,
    FailingProductRepository,
    SpyFallbackRepository,
    SpyProductRepository,
)

from application.abstractions import MarketWebScraper
from application.product_processors import ProductProcessor
from application.use_case import ScrapeMarketsCommandHandler
from domain import Product, Subcategory


class DummyProductProcessor(ProductProcessor):
    @override
    def _process(self, product: Product) -> Product:
        return product


class SpyMarketWebScraper(MarketWebScraper):
    def __init__(self, categories: list[str] | None = None):
        self.had_set_location = False
        self.set_location_called_with: str | None = None
        self.had_gotten_categories = False
        self.__categories = categories or []
        self.had_gotten_subcategories = False
        self.get_subcategories_called_with: list[str] = []
        self.had_scraped_subcategory = False

    @override
    async def set_location(self, postal_code: str) -> None:
        self.had_set_location = True
        self.set_location_called_with = postal_code

    @override
    async def get_categories(self) -> list[str]:
        self.had_gotten_categories = True
        return self.__categories

    @override
    async def get_subcategories(self, category: str) -> list[Subcategory]:
        self.had_gotten_subcategories = True
        self.get_subcategories_called_with.append(category)
        return []

    @override
    async def scrape_subcategory(self, subcategory: Subcategory) -> list[Product]:
        self.had_scraped_subcategory = True
        return []


class FakeMarketWebScraper(SpyMarketWebScraper):
    def __init__(self, products: list[Product]):
        super().__init__(categories=[""])
        self.scrapped_products = products

    @override
    async def get_subcategories(self, category: str) -> list[Subcategory]:
        return [Subcategory(name="subcategory", url="")]

    @override
    async def scrape_subcategory(self, subcategory: Subcategory) -> list[Product]:
        self.had_scraped_subcategory = True
        return self.scrapped_products


def make_handler(**kwargs) -> ScrapeMarketsCommandHandler:
    defaults: dict = dict(
        main_repository=DummyProductRepository(),
        fallback_repository=DummyFallbackRepository(),
        market_web_scrapers={"Market": SpyMarketWebScraper()},
        product_processor=DummyProductProcessor(),
    )
    defaults.update(kwargs)
    return ScrapeMarketsCommandHandler(**defaults)


async def test_sets_location():
    # Arrange
    scraper = SpyMarketWebScraper()
    handler = make_handler(market_web_scrapers={"Market": scraper})

    # Act
    await handler.handle("12345")

    # Assert
    assert scraper.had_set_location


async def test_sets_location_with_given_postal_code():
    # Arrange
    scraper = SpyMarketWebScraper()
    handler = make_handler(market_web_scrapers={"Market": scraper})

    # Act
    await handler.handle("12345")

    # Assert
    assert scraper.set_location_called_with == "12345"


async def test_gets_categories():
    # Arrange
    scraper = SpyMarketWebScraper()
    handler = make_handler(market_web_scrapers={"Market": scraper})

    # Act
    await handler.handle("12345")

    # Assert
    assert scraper.had_gotten_categories


async def test_does_not_get_subcategories_if_no_categories_found():
    # Arrange
    scraper = SpyMarketWebScraper(categories=[])
    handler = make_handler(market_web_scrapers={"Market": scraper})

    # Act
    await handler.handle("12345")

    # Assert
    assert not scraper.had_gotten_subcategories


async def test_gets_subcategories_for_first_category_only():
    # Arrange
    scraper = SpyMarketWebScraper(categories=["category1", "category2"])
    handler = make_handler(market_web_scrapers={"Market": scraper})

    # Act
    await handler.handle("12345")

    # Assert
    assert scraper.get_subcategories_called_with == ["category1"]


async def test_does_not_scrape_subcategory_if_no_subcategories_found():
    # Arrange
    scraper = SpyMarketWebScraper(categories=["category1"])
    handler = make_handler(market_web_scrapers={"Market": scraper})

    # Act
    await handler.handle("12345")

    # Assert
    assert not scraper.had_scraped_subcategory


async def test_scrapes_subcategories_if_found():
    # Arrange
    products = [Product(name="product1", price=1.0, quantity="1 unit")]
    scraper = FakeMarketWebScraper(products)
    handler = make_handler(market_web_scrapers={"Market": scraper})

    # Act
    await handler.handle("12345")

    # Assert
    assert scraper.had_scraped_subcategory


async def test_saves_scrapped_products_to_main_repository():
    # Arrange
    products = [
        Product(name="product1", price=1.0, quantity="1 unit"),
        Product(name="product2", price=2.0, quantity="1 unit"),
    ]
    scraper = FakeMarketWebScraper(products)
    repository = SpyProductRepository()
    handler = make_handler(
        main_repository=repository,
        market_web_scrapers={"Market": scraper},
    )

    # Act
    await handler.handle("12345")

    # Assert
    assert repository.saved_products == products


async def test_saves_to_fallback_if_main_raises():
    # Arrange
    products = [Product(name="product1", price=1.0, quantity="1 unit")]
    scraper = FakeMarketWebScraper(products)
    fallback = SpyFallbackRepository()
    handler = make_handler(
        main_repository=FailingProductRepository(),
        fallback_repository=fallback,
        market_web_scrapers={"Market": scraper},
    )

    # Act
    await handler.handle("12345")

    # Assert
    assert len(fallback.save_calls) == 1
    assert fallback.save_calls[0][2] == products
