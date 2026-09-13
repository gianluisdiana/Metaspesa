import re
from typing import override

from application.product_processors.base import ProductProcessor
from domain import Product


class QuantityUnitOfMeasureExtractor(ProductProcessor):
    def __init__(self) -> None:
        super().__init__()
        amount_pattern = r"(?P<quantity>\d+(?:[.,]\d+)?)"
        unit_pattern = (
            r"(?P<unit>kilogramos?|kilos?|kg|gramos?|gr|mg|g|"
            r"litros?|lav(?:ados)?|ml|cl|l|"
            r"uds?\.?|unidad(?:es)?|"
            r"ds)"
        )
        self.__quantity_pattern = re.compile(
            rf"{amount_pattern}\s*{unit_pattern}(?![a-z])", re.IGNORECASE
        )

    @override
    def _process(self, product: Product) -> Product:
        if not isinstance(product.quantity, str):
            return product

        quantity = product.quantity.strip()
        match = self.__quantity_pattern.search(quantity)
        if match is None:
            return self.__with_quantity(product, 1, self.__fallback_unit(quantity))

        parsed_quantity = float(match.group("quantity").replace(",", "."))
        return self.__with_quantity(product, parsed_quantity, match.group("unit"))

    @staticmethod
    def __fallback_unit(quantity: str) -> str:
        return "kg" if quantity.lower() == "al peso" else "unit"

    @staticmethod
    def __with_quantity(
        product: Product, quantity: float, unit_of_measure: str
    ) -> Product:
        return Product(
            raw_content=product.raw_content,
            name=product.name,
            price=product.price,
            quantity=quantity,
            brand=product.brand,
            image_url=product.image_url,
            unit_of_measure=unit_of_measure,
        )
