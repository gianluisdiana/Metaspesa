from datetime import date
from pathlib import Path

import pytest

from domain import Product
from infrastructure.local_storage import CsvProductRepository


@pytest.mark.parametrize(
    "raw_contents",
    [
        [],
        [""],
        ["Coffee Brand, 500 g, 1.99"],
        ['Café "Brand"; 500 g\n1.99 €', "Milk, 1 l, 0.99"],
    ],
)
async def test_round_trip_preserves_raw_content(
    tmp_path: Path, raw_contents: list[str]
):
    repository = CsvProductRepository(tmp_path)
    products = [
        Product(
            raw_content=raw_content,
            name="Processed name",
            price=1.99,
            quantity=500,
            unit_of_measure="g",
            image_url="https://example.com/product.png",
        )
        for raw_content in raw_contents
    ]
    registered_at = date(2026, 5, 7)

    await repository.save("Market", registered_at, products)
    restored = await repository.get_products_by_market_and_date("Market", registered_at)

    assert [product.raw_content for product in restored] == raw_contents


async def test_quotes_raw_content_and_escapes_embedded_quotes(tmp_path: Path):
    repository = CsvProductRepository(tmp_path)
    product = Product(
        raw_content='Coffee "Brand", 500 g, 1.99',
        name="Coffee",
        price=1.99,
        quantity=500,
        unit_of_measure="g",
        image_url="https://example.com/coffee.png",
    )

    await repository.save("Market", date(2026, 5, 7), [product])

    row = (
        (tmp_path / "2026-05-07_market.csv").read_text(encoding="utf-8").splitlines()[1]
    )
    assert row.startswith('"Coffee ""Brand"", 500 g, 1.99";')


async def test_saves_and_reads_products_with_csv_escaping(tmp_path: Path):
    # Arrange
    repository = CsvProductRepository(tmp_path)
    products = [
        Product(
            raw_content="Original product, 1 unit, 1.0",
            name='Product "Name"; Extra',
            price=1.99,
            quantity=500,
            unit_of_measure="g",
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

    # Cleanup
    await repository.remove_old_products("Market", today)


async def test_preserves_image_url_when_reading_products(tmp_path: Path):
    # Arrange
    repository = CsvProductRepository(tmp_path)
    products = [
        Product(
            raw_content="Original product, 1 unit, 1.0",
            name='Product "Name"; Extra',
            price=1.99,
            quantity=500,
            unit_of_measure="g",
            brand=None,
            image_url="https://example.com/product.png",
        )
    ]
    today = date(2026, 5, 7)
    await repository.save("Market", today, products)

    # Act
    saved_products = await repository.get_products_by_market_and_date("Market", today)

    # Assert
    assert saved_products[0].image_url == "https://example.com/product.png"

    # Cleanup
    await repository.remove_old_products("Market", today)


async def test_preserves_unit_of_measure_when_reading_products(tmp_path: Path):
    # Arrange
    repository = CsvProductRepository(tmp_path)
    products = [
        Product(
            raw_content="Original product, 1 unit, 1.0",
            name="Product",
            price=1.99,
            quantity=1000,
            unit_of_measure="g",
            brand="Brand",
            image_url="https://example.com/product.png",
        )
    ]
    today = date(2026, 5, 7)
    await repository.save("Market", today, products)

    # Act
    saved_products = await repository.get_products_by_market_and_date("Market", today)

    # Assert
    assert saved_products[0].unit_of_measure == "g"


async def test_ignores_invalid_csv_filenames_when_getting_markets_and_dates(
    tmp_path: Path,
):
    # Arrange
    repository = CsvProductRepository(tmp_path)
    (tmp_path / "invalid.csv").write_text("", encoding="utf-8")
    (tmp_path / "2026-05-07_market_extra.csv").write_text("", encoding="utf-8")

    # Act
    markets_and_dates = await repository.get_markets_and_dates()

    # Assert
    assert markets_and_dates == []


async def test_ignores_malformed_dates_when_getting_markets_and_dates(tmp_path: Path):
    # Arrange
    repository = CsvProductRepository(tmp_path)
    (tmp_path / "not-a-date_market.csv").write_text("", encoding="utf-8")

    # Act
    markets_and_dates = await repository.get_markets_and_dates()

    # Assert
    assert markets_and_dates == []


async def test_reads_empty_file_as_no_products(tmp_path: Path):
    # Arrange
    repository = CsvProductRepository(tmp_path)
    today = date(2026, 5, 7)
    (tmp_path / "2026-05-07_market.csv").write_text("", encoding="utf-8")

    # Act
    products = await repository.get_products_by_market_and_date("Market", today)

    # Assert
    assert products == []


async def test_gets_multiple_markets_and_dates(tmp_path: Path):
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
