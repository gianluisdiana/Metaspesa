import math

import pytest

from application.product_processors import QuantityUnitOfMeasureExtractor
from domain import Product


def test_extracts_quantity():
    # Arrange
    extractor = QuantityUnitOfMeasureExtractor()
    product = Product(
        name="Product",
        price=1.0,
        quantity="2g",
        image_url="https://example.com/product.png",
    )

    # Act
    result = extractor.process(product)

    # Assert
    assert math.isclose(float(result.quantity), 2.0)


def test_extracts_quantity_more_than_1_digit():
    # Arrange
    extractor = QuantityUnitOfMeasureExtractor()
    product = Product(
        name="Product",
        price=1.0,
        quantity="10kg",
        image_url="https://example.com/product.png",
    )

    # Act
    result = extractor.process(product)

    # Assert
    assert math.isclose(float(result.quantity), 10)


@pytest.mark.parametrize(
    "raw_quantity,expected_quantity",
    [("1.1g", 1.1), ("1,1g", 1.1)],
)
def test_extracts_quantity_with_decimal(raw_quantity: str, expected_quantity: float):
    # Arrange
    extractor = QuantityUnitOfMeasureExtractor()
    product = Product(
        name="Product",
        price=1.0,
        quantity=raw_quantity,
        image_url="https://example.com/product.png",
    )

    # Act
    result = extractor.process(product)

    # Assert
    assert math.isclose(float(result.quantity), expected_quantity)


@pytest.mark.parametrize(
    "raw_quantity,expected_unit",
    [
        ("1kilogramo", "kilogramo"),
        ("1kilogramos", "kilogramos"),
        ("1kilos", "kilos"),
        ("1kilo", "kilo"),
        ("1kg", "kg"),
        ("1gramos", "gramos"),
        ("1gramo", "gramo"),
        ("1gr", "gr"),
        ("1mg", "mg"),
        ("1g", "g"),
        ("1litros", "litros"),
        ("1litro", "litro"),
        ("1lavados", "lavados"),
        ("1lav", "lav"),
        ("1ml", "ml"),
        ("1cl", "cl"),
        ("1l", "l"),
        ("1ud", "ud"),
        ("1ud.", "ud."),
        ("1uds", "uds"),
        ("1uds.", "uds."),
        ("1unidad", "unidad"),
        ("1unidades", "unidades"),
        ("1ds", "ds"),
    ],
)
def test_extracts_quantity_and_unit_of_measure(raw_quantity: str, expected_unit: str):
    # Arrange
    extractor = QuantityUnitOfMeasureExtractor()
    product = Product(
        name="Product",
        price=1.0,
        quantity=raw_quantity,
        image_url="https://example.com/product.png",
    )

    # Act
    result = extractor.process(product)

    # Assert
    assert result.unit_of_measure == expected_unit


def test_does_not_extract_and_assigns_default_quantity():
    # Arrange
    extractor = QuantityUnitOfMeasureExtractor()
    product = Product(
        name="Product",
        price=1.0,
        quantity="bandeja",
        image_url="https://example.com/product.png",
    )

    # Act
    result = extractor.process(product)

    # Assert
    assert result.quantity == 1


def test_does_not_extract_and_assigns_kg_to_unit_of_measure():
    # Arrange
    extractor = QuantityUnitOfMeasureExtractor()
    product = Product(
        name="Product",
        price=1.0,
        quantity="al peso",
        image_url="https://example.com/product.png",
    )

    # Act
    result = extractor.process(product)

    # Assert
    assert result.unit_of_measure == "kg"


def test_does_not_extract_and_assigns_default_unit_of_measure():
    # Arrange
    extractor = QuantityUnitOfMeasureExtractor()
    product = Product(
        name="Product",
        price=1.0,
        quantity="bandeja",
        image_url="https://example.com/product.png",
    )

    # Act
    result = extractor.process(product)

    # Assert
    assert result.unit_of_measure == "unit"
