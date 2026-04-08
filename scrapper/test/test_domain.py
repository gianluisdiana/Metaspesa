from typing import Any

import pytest

from domain import Product


@pytest.mark.parametrize(
    "non_product",
    [
        1,
        1.0,
        "Not a product",
        ["Not", "a", "product"],
        {"Not": "a product"},
        (1, "Not a product"),
        None,
    ],
)
def test_is_not_equal_to_non_product(non_product: Any):
    # Arrange
    product = Product(name="Product", price=1.0, quantity="1 unit")

    # Act
    are_not_equal = product != non_product

    # Assert
    assert are_not_equal


def test_is_equal_to_product_with_same_attributes():
    # Arrange
    product1 = Product(name="Product", price=1.0, quantity="1 unit", brand="Brand")
    product2 = Product(name="Product", price=1.0, quantity="1 unit", brand="Brand")

    # Act
    are_equal = product1 == product2

    # Assert
    assert are_equal


def test_is_not_equal_to_product_with_different_name():
    # Arrange
    product1 = Product(name="Product 1", price=1.0, quantity="1 unit", brand="Brand")
    product2 = Product(name="Product 2", price=1.0, quantity="1 unit", brand="Brand")

    # Act
    are_not_equal = product1 != product2

    # Assert
    assert are_not_equal


def test_is_not_equal_to_product_with_different_price():
    # Arrange
    product1 = Product(name="Product", price=1.0, quantity="1 unit", brand="Brand")
    product2 = Product(name="Product", price=2.0, quantity="1 unit", brand="Brand")

    # Act
    are_not_equal = product1 != product2

    # Assert
    assert are_not_equal


def test_is_equal_to_product_with_close_enough_prices():
    # Arrange
    product1 = Product(name="Product", price=1.0, quantity="1 unit", brand="Brand")
    product2 = Product(name="Product", price=1.0001, quantity="1 unit", brand="Brand")

    # Act
    are_equal = product1 == product2

    # Assert
    assert are_equal


def test_is_not_equal_to_product_with_different_quantity():
    # Arrange
    product1 = Product(name="Product", price=1.0, quantity="1 unit", brand="Brand")
    product2 = Product(name="Product", price=1.0, quantity="2 units", brand="Brand")

    # Act
    are_not_equal = product1 != product2

    # Assert
    assert are_not_equal


def test_is_not_equal_to_product_with_different_brand():
    # Arrange
    product1 = Product(name="Product", price=1.0, quantity="1 unit", brand="Brand 1")
    product2 = Product(name="Product", price=1.0, quantity="1 unit", brand="Brand 2")

    # Act
    are_not_equal = product1 != product2

    # Assert
    assert are_not_equal


def test_hash_of_equal_products_is_the_same():
    # Arrange
    product1 = Product(name="Product", price=1.0, quantity="1 unit", brand="Brand")
    product2 = Product(name="Product", price=1.0, quantity="1 unit", brand="Brand")

    # Act
    hash1 = hash(product1)
    hash2 = hash(product2)

    # Assert
    assert hash1 == hash2
