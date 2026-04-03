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
    product = Product(name=f"{brand} Leche entera 1L", price=2.5, quantity="1 ud")

    # Act
    result = extractor.process(product)

    # Assert
    assert result.brand == expected_brand


def test_brand_extractor_does_not_extract_brand_for_unknown_product():
    # Arrange
    extractor = BrandExtractor(["Hacendado"])
    product = Product(name="Unknown Brand Yogurt", price=1.5, quantity="1 ud")

    # Act
    result = extractor.process(product)

    # Assert
    assert result.brand is None
