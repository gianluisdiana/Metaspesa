import pytest

from application.product_processors import BrandExtractor
from domain import Product


@pytest.mark.parametrize(
    "brand",
    [
        "Hacendado",
        "hacendado",  # case-insensitive
        "Hacendado como prefijo",  # prefix
        "Sufijo Hacendado",  # suffix
        "Hacendado-like",  # partial
    ],
)
def test_brand_extractor_recognizes_known_brands(brand: str):
    # Arrange
    expected_brand = "Hacendado"
    extractor = BrandExtractor([expected_brand])
    product = Product(
        raw_content="Original product, 1 unit, 1.0",
        name=f"{brand} Leche entera 1L",
        price=2.5,
        quantity="1 ud",
        image_url="https://example.com/product.png",
    )

    # Act
    result = extractor.process(product)

    # Assert
    assert result.brand == expected_brand


def test_brand_extractor_does_not_extract_brand_for_unknown_product():
    # Arrange
    extractor = BrandExtractor(["Hacendado"])
    product = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Unknown Brand Yogurt",
        price=1.5,
        quantity="1 ud",
        image_url="https://example.com/product.png",
    )

    # Act
    result = extractor.process(product)

    # Assert
    assert result.brand is None


@pytest.mark.parametrize(
    "name, brand",
    [("Brand Coffee", None), ("Coffee", None), ("Brand Coffee", "Existing")],
)
def test_preserves_raw_content(name: str, brand: str | None) -> None:
    processor = BrandExtractor(["Brand"])
    product = Product(
        raw_content="Café Variant, 500 g, 1.99 €",
        name=name,
        quantity=500,
        unit_of_measure="g",
        brand=brand,
        price=1.99,
        image_url="https://example.com/coffee.png",
    )

    result = processor.process(product)

    assert result.raw_content == "Café Variant, 500 g, 1.99 €"
