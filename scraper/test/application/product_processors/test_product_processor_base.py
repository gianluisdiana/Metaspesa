from application.product_processors import ProductProcessor
from domain import Product


class SpyProcessor(ProductProcessor):
    def __init__(self):
        super().__init__()
        self.has_processed_products = False

    def _process(self, product: Product) -> Product:
        self.has_processed_products = True
        return product


class RenamingProcessor(ProductProcessor):
    def __init__(self, suffix: str):
        super().__init__()
        self.__suffix = suffix

    def _process(self, product: Product) -> Product:
        return Product(
            name=f"{product.name}{self.__suffix}",
            price=product.price,
            quantity=product.quantity,
            image_url=product.image_url,
            brand=product.brand,
        )


def test_processor_processes_product():
    # Arrange
    processor = SpyProcessor()
    product = Product(
        name="apple",
        price=1.0,
        quantity="1 kg",
        image_url="https://example.com/product.png",
    )

    # Act
    processor.process(product)

    # Assert
    assert processor.has_processed_products


def test_chain_of_responsibility_processes_first_processor():
    # Arrange
    p1 = SpyProcessor()
    p2 = SpyProcessor()
    p1.next(p2)

    product = Product(
        name="apple",
        price=1.0,
        quantity="1 kg",
        image_url="https://example.com/product.png",
    )

    # Act
    p1.process(product)

    # Assert
    assert p1.has_processed_products


def test_chain_of_responsibility_processes_next_processor():
    # Arrange
    p1 = SpyProcessor()
    p2 = SpyProcessor()
    p1.next(p2)

    product = Product(
        name="apple",
        price=1.0,
        quantity="1 kg",
        image_url="https://example.com/product.png",
    )

    # Act
    p1.process(product)

    # Assert
    assert p2.has_processed_products


def test_chain_of_responsibility_returns_final_product():
    # Arrange
    p1 = RenamingProcessor(" first")
    p2 = RenamingProcessor(" second")
    p1.next(p2)

    product = Product(
        name="apple",
        price=1.0,
        quantity="1 kg",
        image_url="https://example.com/product.png",
    )

    # Act
    result = p1.process(product)

    # Assert
    assert result.name == "apple first second"
