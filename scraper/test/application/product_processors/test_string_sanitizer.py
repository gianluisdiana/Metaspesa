import pytest

from application.product_processors import ProductProcessor, StringSanitizer
from domain import Product


def test_string_sanitizer_removes_non_ascii_from_name():
    # Arrange
    sanitizer = StringSanitizer()
    product = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Café ☕",
        price=1.0,
        quantity="1 unit",
        image_url="https://example.com/product.png",
    )

    # Act
    result = sanitizer.process(product)

    # Assert
    assert result.name == "Cafe "


def test_string_sanitizer_removes_non_ascii_from_quantity():
    # Arrange
    sanitizer = StringSanitizer()
    product = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Coffee",
        price=1.0,
        quantity="500 g ✓",
        image_url="https://example.com/product.png",
    )

    # Act
    result = sanitizer.process(product)

    # Assert
    assert result.quantity == "500 g "


def test_string_sanitizer_removes_non_ascii_from_brand():
    # Arrange
    sanitizer = StringSanitizer()
    product = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Coffee",
        price=1.0,
        quantity="500 g",
        image_url="https://example.com/product.png",
        brand="Niño",
    )

    # Act
    result = sanitizer.process(product)

    # Assert
    assert result.brand == "Nino"


def test_string_sanitizer_keeps_missing_brand():
    # Arrange
    sanitizer = StringSanitizer()
    product = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Coffee",
        price=1.0,
        quantity="500 g",
        image_url="https://example.com/product.png",
    )

    # Act
    result = sanitizer.process(product)

    # Assert
    assert result.brand is None


def test_string_sanitizer_removes_non_ascii_from_image_url():
    # Arrange
    sanitizer = StringSanitizer()
    product = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Coffee",
        price=1.0,
        quantity="500 g",
        image_url="https://example.com/café.png",
    )

    # Act
    result = sanitizer.process(product)

    # Assert
    assert result.image_url == "https://example.com/cafe.png"


def test_string_sanitizer_sends_product_to_next_processor():
    # Arrange
    class CapturingProcessor(ProductProcessor):
        def __init__(self):
            super().__init__()
            self.received_product: Product | None = None

        def _process(self, product: Product) -> Product:
            self.received_product = product
            return product

    sanitizer = StringSanitizer()
    next_processor = CapturingProcessor()
    sanitizer.next(next_processor)
    product = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Café ☕",
        price=1.0,
        quantity="500 g ✓",
        image_url="https://example.com/product.png",
    )

    # Act
    sanitizer.process(product)

    # Assert
    assert next_processor.received_product is not None


def test_string_sanitizer_sanitizes_name_before_next_processor():
    # Arrange
    class CapturingProcessor(ProductProcessor):
        def __init__(self):
            super().__init__()
            self.received_product: Product | None = None

        def _process(self, product: Product) -> Product:
            self.received_product = product
            return product

    sanitizer = StringSanitizer()
    next_processor = CapturingProcessor()
    sanitizer.next(next_processor)
    product = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Café ☕",
        price=1.0,
        quantity="500 g ✓",
        image_url="https://example.com/product.png",
    )

    # Act
    sanitizer.process(product)

    # Assert
    assert next_processor.received_product.name == "Cafe "


def test_string_sanitizer_sanitizes_quantity_before_next_processor():
    # Arrange
    class CapturingProcessor(ProductProcessor):
        def __init__(self):
            super().__init__()
            self.received_product: Product | None = None

        def _process(self, product: Product) -> Product:
            self.received_product = product
            return product

    sanitizer = StringSanitizer()
    next_processor = CapturingProcessor()
    sanitizer.next(next_processor)
    product = Product(
        raw_content="Original product, 1 unit, 1.0",
        name="Café ☕",
        price=1.0,
        quantity="500 g ✓",
        image_url="https://example.com/product.png",
    )

    # Act
    sanitizer.process(product)

    # Assert
    assert next_processor.received_product.quantity == "500 g "


@pytest.mark.parametrize(
    "raw_content, expected",
    [
        ("", ""),
        ("Coffee BRAND, 500 g, 1.99", "Coffee BRAND, 500 g, 1.99"),
        ("Café Niño, 500 g, 1.99 € ☕", "Cafe Nino, 500 g, 1.99  "),
        ("Cafe\u0301, 1 kg, 2.0", "Cafe, 1 kg, 2.0"),
    ],
)
def test_sanitizes_raw_content_independently_of_processed_name(
    raw_content: str, expected: str
) -> None:
    product = Product(
        raw_content=raw_content,
        name="Processed coffee",
        price=1.99,
        quantity=500,
        image_url="https://example.com/coffee.png",
    )

    result = StringSanitizer().process(product)

    assert result.raw_content == expected


def test_passes_sanitized_raw_content_to_next_processor():
    class RawContentReader(ProductProcessor):
        def _process(self, product: Product) -> Product:
            assert product.raw_content == "Cafe, 500 g, 1.99 "
            return product

    processor = StringSanitizer()
    processor.next(RawContentReader())
    product = Product(
        raw_content="Café, 500 g, 1.99 €",
        name="Coffee",
        price=1.99,
        quantity="500 g",
        image_url="https://example.com/coffee.png",
    )

    processor.process(product)
