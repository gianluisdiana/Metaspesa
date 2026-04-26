from typing import override

from application.abstractions import ProductRepository
from application.app_product_repository import AppProductRepository
from domain import Product


class SpyProductRepository(ProductRepository):
    def __init__(self):
        self.saved_products: list[Product] = []

    @override
    async def save(self, market_name: str, products: list[Product]) -> None:
        self.saved_products = products


class FailingProductRepository(ProductRepository):
    @override
    async def save(self, market_name: str, products: list[Product]) -> None:
        raise Exception("Failed to save products")


async def test_saves_products_in_main_repository():
    # Arrange
    market_name = "Market"
    products = [
        Product(name="product1", price=1.0, quantity="1 unit"),
        Product(name="product2", price=2.0, quantity="1 unit"),
    ]
    main_repository = SpyProductRepository()
    repository = AppProductRepository(main_repository, SpyProductRepository())

    # Act
    await repository.save(market_name, products)

    # Assert
    assert main_repository.saved_products == products


async def test_saves_products_in_fallback_repository_on_failure():
    # Arrange
    market_name = "Market"
    products = [
        Product(name="product1", price=1.0, quantity="1 unit"),
        Product(name="product2", price=2.0, quantity="1 unit"),
    ]
    main_repository = FailingProductRepository()
    fallback_repository = SpyProductRepository()
    repository = AppProductRepository(main_repository, fallback_repository)

    # Act
    await repository.save(market_name, products)

    # Assert
    assert fallback_repository.saved_products == products
