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


async def test_ignores_invalid_csv_filenames_when_getting_markets_and_dates(tmp_path):
    # Arrange
    repository = CsvProductRepository(tmp_path)
    (tmp_path / "invalid.csv").write_text("", encoding="utf-8")
    (tmp_path / "2026-05-07_market_extra.csv").write_text("", encoding="utf-8")

    # Act
    markets_and_dates = await repository.get_markets_and_dates()

    # Assert
    assert markets_and_dates == []


async def test_ignores_malformed_dates_when_getting_markets_and_dates(tmp_path):
    # Arrange
    repository = CsvProductRepository(tmp_path)
    (tmp_path / "not-a-date_market.csv").write_text("", encoding="utf-8")

    # Act
    markets_and_dates = await repository.get_markets_and_dates()

    # Assert
    assert markets_and_dates == []


async def test_reads_empty_file_as_no_products(tmp_path):
    # Arrange
    repository = CsvProductRepository(tmp_path)
    today = date(2026, 5, 7)
    (tmp_path / "2026-05-07_market.csv").write_text("", encoding="utf-8")

    # Act
    products = await repository.get_products_by_market_and_date("Market", today)

    # Assert
    assert products == []


async def test_gets_multiple_markets_and_dates(tmp_path):
    # Arrange
    repository = CsvProductRepository(tmp_path)
    await repository.save("Market1", date(2026, 5, 7), [])
    await repository.save("Market2", date(2026, 5, 8), [])

    # Act
    markets_and_dates = await repository.get_markets_and_dates()

    # Assert
    assert sorted(markets_and_dates) == [
        ("market1", date(2026, 5, 7)),
        ("market2", date(2026, 5, 8)),
    ]
