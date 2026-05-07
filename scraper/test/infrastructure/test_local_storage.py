from datetime import date

from domain import Product
from infrastructure.local_storage import CsvProductRepository


async def test_saves_and_reads_products_with_csv_escaping(tmp_path):
    # Arrange
    repository = CsvProductRepository(tmp_path)
    products = [
        Product(
            name='Product "Name"; Extra',
            price=1.99,
            quantity="500g",
            brand=None,
            image_url="https://example.com/product.png",
        )
    ]
    today = date(2026, 5, 7)
    await repository.save("Market", today, products)

    # Act
    saved_products = await repository.get_products_by_market_and_date("Market", today)

    # Assert
    assert saved_products == products
    assert saved_products[0].image_url == "https://example.com/product.png"

    # Cleanup
    await repository.remove_old_products("Market", today)
