import csv
from datetime import date
from pathlib import Path
from typing import override

from application.abstractions import FallbackProductRepository
from domain import Product


class CsvProductRepository(FallbackProductRepository):
    def __init__(self, output_dir: Path) -> None:
        self.__output_dir = output_dir

        if not self.__output_dir.exists():
            self.__output_dir.mkdir(parents=True)

    @override
    async def save(self, market_name: str, date: date, products: list[Product]) -> None:
        file_name = self.__create_file_name(market_name, date)
        output_file = self.__output_dir / file_name

        with output_file.open("w", encoding="utf-8", newline="") as f:
            writer = csv.writer(f, delimiter=";")
            writer.writerow(["Name", "Price", "Quantity", "Brand", "ImageURL"])
            writer.writerows(
                [
                    product.name,
                    product.price,
                    product.quantity,
                    product.brand,
                    product.image_url,
                ]
                for product in products
            )

    @override
    async def get_markets_and_dates(self) -> list[tuple[str, date]]:
        markets_and_dates: list[tuple[str, date]] = []
        for file in self.__output_dir.glob("*.csv"):
            name_parts = file.stem.split("_")
            if len(name_parts) != 2:
                continue

            market_name = name_parts[1]
            try:
                registered_at = date.fromisoformat(name_parts[0])
            except ValueError:
                continue
            markets_and_dates.append((market_name, registered_at))

        return markets_and_dates

    @override
    async def get_products_by_market_and_date(
        self, market_name: str, date: date
    ) -> list[Product]:
        file_name = self.__create_file_name(market_name, date)
        input_file = self.__output_dir / file_name

        if not input_file.exists():
            return []

        products: list[Product] = []
        with input_file.open("r", encoding="utf-8", newline="") as f:
            reader = csv.DictReader(f, delimiter=";")
            for row in reader:
                brand = row["Brand"]
                products.append(
                    Product(
                        name=row["Name"],
                        price=float(row["Price"]),
                        quantity=row["Quantity"],
                        brand=brand if brand != "" else None,
                        image_url=row["ImageURL"],
                    )
                )

        return products

    @override
    async def remove_old_products(self, market_name: str, date: date) -> None:
        file_name = self.__create_file_name(market_name, date)
        output_file = self.__output_dir / file_name

        if output_file.exists():
            output_file.unlink()

    @staticmethod
    def __create_file_name(market_name: str, date: date) -> str:
        return f"{date.isoformat()}_{market_name.lower()}.csv"
