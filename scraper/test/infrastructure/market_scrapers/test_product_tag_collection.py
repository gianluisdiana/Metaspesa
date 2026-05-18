from domain import Product
from infrastructure.market_scrapers.product_tags import ProductTagCollection


class FakeProductTag:
    def __init__(self, product: Product, ready: bool = True) -> None:
        self.__product = product
        self.__ready = ready

    def is_ready(self) -> bool:
        return self.__ready

    def to_product(self) -> Product:
        return self.__product


def product(name: str) -> Product:
    return Product(
        name=name,
        price=1.0,
        quantity="1 unit",
        image_url="https://example.com/product.png",
    )


def test_returns_ready_products_after_last_added_product():
    # Arrange
    product1 = product("Product 1")
    product2 = product("Product 2")
    product3 = product("Product 3")
    collection = ProductTagCollection(
        [
            FakeProductTag(product1),
            FakeProductTag(product2),
            FakeProductTag(product3),
        ]
    )

    # Act
    new_products = collection.new_products_since(product1)

    # Assert
    assert new_products == [product2, product3]


def test_ignores_not_ready_products():
    # Arrange
    ready_product = product("Ready")
    not_ready_product = product("Not ready")
    collection = ProductTagCollection(
        [
            FakeProductTag(ready_product),
            FakeProductTag(not_ready_product, ready=False),
        ]
    )

    # Act
    new_products = collection.new_products_since(None)

    # Assert
    assert new_products == [ready_product]
