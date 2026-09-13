from dataclasses import replace
from typing import Any

import pytest

from domain import Product


def test_raw_content_does_not_change_product_equality():
    product = Product(
        raw_content="Coffee Brand, 500 g, 1.99",
        name="Coffee",
        price=1.99,
        quantity=500,
        image_url="https://example.com/coffee.png",
    )
    same_product = replace(product, raw_content="Brand Coffee, 500g, 1.99")

    assert product == same_product


def test_raw_content_does_not_change_product_hash():
    product = Product(
        raw_content="Coffee Brand, 500 g, 1.99",
        name="Coffee",
        price=1.99,
        quantity=500,
        image_url="https://example.com/coffee.png",
    )
    same_product = replace(product, raw_content="Brand Coffee, 500g, 1.99")

    assert hash(product) == hash(same_product)


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
    product = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=1.0,
        quantity="1 unit",
        image_url="https://example.com/product.png",
    )

    # Act
    are_not_equal = product != non_product

    # Assert
    assert are_not_equal


def test_is_equal_to_product_with_same_attributes():
    # Arrange
    product1 = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=1.0,
        quantity="1 unit",
        image_url="https://example.com/product.png",
        brand="Brand",
    )
    product2 = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=1.0,
        quantity="1 unit",
        image_url="https://example.com/product.png",
        brand="Brand",
    )

    # Act
    are_equal = product1 == product2

    # Assert
    assert are_equal


def test_is_not_equal_to_product_with_different_name():
    # Arrange
    product1 = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product 1",
        price=1.0,
        quantity="1 unit",
        image_url="https://example.com/product.png",
        brand="Brand",
    )
    product2 = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product 2",
        price=1.0,
        quantity="1 unit",
        image_url="https://example.com/product.png",
        brand="Brand",
    )

    # Act
    are_not_equal = product1 != product2

    # Assert
    assert are_not_equal


def test_is_not_equal_to_product_with_different_price():
    # Arrange
    product1 = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=1.0,
        quantity="1 unit",
        image_url="https://example.com/product.png",
        brand="Brand",
    )
    product2 = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=2.0,
        quantity="1 unit",
        image_url="https://example.com/product.png",
        brand="Brand",
    )

    # Act
    are_not_equal = product1 != product2

    # Assert
    assert are_not_equal


def test_is_equal_to_product_with_close_enough_prices():
    # Arrange
    product1 = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=1.0,
        quantity="1 unit",
        image_url="https://example.com/product.png",
        brand="Brand",
    )
    product2 = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=1.0001,
        quantity="1 unit",
        image_url="https://example.com/product.png",
        brand="Brand",
    )

    # Act
    are_equal = product1 == product2

    # Assert
    assert are_equal


def test_is_not_equal_to_product_with_different_quantity():
    # Arrange
    product1 = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=1.0,
        quantity="1 unit",
        image_url="https://example.com/product.png",
        brand="Brand",
    )
    product2 = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=1.0,
        quantity="2 units",
        image_url="https://example.com/product.png",
        brand="Brand",
    )

    # Act
    are_not_equal = product1 != product2

    # Assert
    assert are_not_equal


def test_is_not_equal_to_product_with_different_brand():
    # Arrange
    product1 = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=1.0,
        quantity="1 unit",
        image_url="https://example.com/product.png",
        brand="Brand 1",
    )
    product2 = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=1.0,
        quantity="1 unit",
        image_url="https://example.com/product.png",
        brand="Brand 2",
    )

    # Act
    are_not_equal = product1 != product2

    # Assert
    assert are_not_equal


def test_hash_of_equal_products_is_the_same():
    # Arrange
    product1 = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=1.0,
        quantity="1 unit",
        image_url="https://example.com/product.png",
        brand="Brand",
    )
    product2 = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=1.0,
        quantity="1 unit",
        image_url="https://example.com/product.png",
        brand="Brand",
    )

    # Act
    hash1 = hash(product1)
    hash2 = hash(product2)

    # Assert
    assert hash1 == hash2
