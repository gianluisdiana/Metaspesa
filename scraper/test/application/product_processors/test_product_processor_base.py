from application.product_processors import ProductProcessor
from domain import Product


class SpyProcessor(ProductProcessor):
    def __init__(self):
        super().__init__()
        self.has_processed_products = False

    def _process(self, product: Product) -> Product:
        self.has_processed_products = True
        return product


def test_processor_processes_product():
    # Arrange
    processor = SpyProcessor()
    product = Product(name="apple", price=1.0, quantity="1 kg")

    # Act
    processor.process(product)

    # Assert
    assert processor.has_processed_products


def test_chain_of_responsibility_processes_in_sequence():
    # Arrange
    p1 = SpyProcessor()
    p2 = SpyProcessor()
    p1.next(p2)

    product = Product(name="apple", price=1.0, quantity="1 kg")

    # Act
    p1.process(product)

    # Assert
    assert p1.has_processed_products and p2.has_processed_products


def test_chain_of_responsibility_returns_final_product():
    # Arrange
    class ModifyingProcessor(ProductProcessor):
        def _process(self, product: Product) -> Product:
            return Product(
                name=product.name + " modified",
                price=product.price,
                quantity=product.quantity,
            )

    p1 = ModifyingProcessor()
    p2 = SpyProcessor()
    p1.next(p2)

    product = Product(name="apple", price=1.0, quantity="1 kg")
    modified_product = p1.process(p2.process(product))

    # Act
    result = p1.process(product)

    # Assert
    assert result == modified_product
