from datetime import datetime
from pathlib import Path
from typing import override

from application.abstractions import ProductRepository
from domain import Product


class CsvProductRepository(ProductRepository):
    def __init__(self, output_dir: Path) -> None:
        self.__output_dir = output_dir
        self.__today = datetime.now().strftime("%Y-%m-%d")

        if not self.__output_dir.exists():
            self.__output_dir.mkdir(parents=True)

    @override
    async def save(self, market_name: str, products: list[Product]) -> None:
        output_file = self.__output_dir / f"{self.__today}_{market_name.lower()}.csv"

        with output_file.open("w", encoding="utf-8") as f:
            f.write("Name;Price;Quantity;Brand\n")
            for product in products:
                f.write(
                    f'"{product.name}";{product.price};{product.quantity};{product.brand}\n'
                )
