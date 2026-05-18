from application.product_processors import ProductProcessor, StringSanitizer
from domain import Product


def test_string_sanitizer_removes_non_ascii_from_name():
    # Arrange
    sanitizer = StringSanitizer()
    product = Product(
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
        name="Café ☕",
        price=1.0,
        quantity="500 g ✓",
        image_url="https://example.com/product.png",
    )

    # Act
    sanitizer.process(product)

    # Assert
    assert next_processor.received_product.quantity == "500 g "
