import pytest

from application.product_processors import UnitOfMeasureNormalizer
from domain import Product


@pytest.mark.parametrize(
    "unit,expected_quantity",
    [
        ("kg", 1000),
        ("mg", 0.001),
    ],
)
def test_converts_weight_quantities_to_grams(unit: str, expected_quantity: float):
    # Arrange
    product = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=1.0,
        quantity=1,
        unit_of_measure=unit,
        image_url="https://example.com/product.png",
    )

    # Act
    result = UnitOfMeasureNormalizer().process(product)

    # Assert
    assert result.quantity == expected_quantity


@pytest.mark.parametrize(
    "unit,expected_quantity",
    [
        ("L", 1000),
        ("cl", 10),
    ],
)
def test_converts_volume_quantities_to_milliliters(unit: str, expected_quantity: float):
    # Arrange
    product = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=1.0,
        quantity=1,
        unit_of_measure=unit,
        image_url="https://example.com/product.png",
    )

    # Act
    result = UnitOfMeasureNormalizer().process(product)

    # Assert
    assert result.quantity == expected_quantity


@pytest.mark.parametrize("unit", ["g", "gr", "gramo", "gramos", "kg"])
def test_normalizes_weight_units_to_grams(unit: str):
    # Arrange
    product = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=1.0,
        quantity=1,
        unit_of_measure=unit,
        image_url="https://example.com/product.png",
    )

    # Act
    result = UnitOfMeasureNormalizer().process(product)

    # Assert
    assert result.unit_of_measure == "g"


@pytest.mark.parametrize("unit", ["ml", "cl", "L", "litro", "litros"])
def test_normalizes_volume_units_to_milliliters(unit: str):
    # Arrange
    product = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=1.0,
        quantity=1,
        unit_of_measure=unit,
        image_url="https://example.com/product.png",
    )

    # Act
    result = UnitOfMeasureNormalizer().process(product)

    # Assert
    assert result.unit_of_measure == "ml"


@pytest.mark.parametrize("unit", ["ud", "ud.", "uds", "uds.", "unidad", "unidades"])
def test_normalizes_unit_count_aliases(unit: str):
    # Arrange
    product = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=1.0,
        quantity=1,
        unit_of_measure=unit,
        image_url="https://example.com/product.png",
    )

    # Act
    result = UnitOfMeasureNormalizer().process(product)

    # Assert
    assert result.unit_of_measure == "unit"


@pytest.mark.parametrize(
    "unit,expected_unit",
    [
        ("lav", "wash"),
        ("lavados", "wash"),
        ("ds", "dose"),
    ],
)
def test_normalizes_non_weight_volume_units(unit: str, expected_unit: str):
    # Arrange
    product = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Product",
        price=1.0,
        quantity=1,
        unit_of_measure=unit,
        image_url="https://example.com/product.png",
    )

    # Act
    result = UnitOfMeasureNormalizer().process(product)

    # Assert
    assert result.unit_of_measure == expected_unit


@pytest.mark.parametrize(
    "quantity, unit",
    [(1, "kg"), (500, "unknown"), ("500 g", "")],
)
def test_preserves_raw_content(quantity: float | str, unit: str) -> None:
    processor = UnitOfMeasureNormalizer()
    product = Product(
        raw_content="Café Variant, 500 g, 1.99 €",
        name="Coffee",
        quantity=quantity,
        unit_of_measure=unit,
        price=1.99,
        image_url="https://example.com/coffee.png",
    )

    result = processor.process(product)

    assert result.raw_content == "Café Variant, 500 g, 1.99 €"
