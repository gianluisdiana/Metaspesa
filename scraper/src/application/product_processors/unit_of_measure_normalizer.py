from typing import ClassVar, override

from application.product_processors.base import ProductProcessor
from domain import Product


class UnitOfMeasureNormalizer(ProductProcessor):
    __UNIT_ALIASES: ClassVar[dict[str, str]] = {
        "g": "g",
        "gr": "g",
        "gramo": "g",
        "gramos": "g",
        "kg": "kg",
        "kilo": "kg",
        "kilos": "kg",
        "kilogramo": "kg",
        "kilogramos": "kg",
        "mg": "mg",
        "ml": "ml",
        "cl": "cl",
        "l": "l",
        "litro": "l",
        "litros": "l",
        "ud": "unit",
        "ud.": "unit",
        "uds": "unit",
        "uds.": "unit",
        "unidad": "unit",
        "unidades": "unit",
        "lav": "wash",
        "lavados": "wash",
        "ds": "dose",
        "unit": "unit",
    }

    __CONVERSIONS: ClassVar[dict[str, tuple[float, str]]] = {
        "mg": (0.001, "g"),
        "kg": (1000, "g"),
        "cl": (10, "ml"),
        "l": (1000, "ml"),
    }

    @override
    def _process(self, product: Product) -> Product:
        unit = self.__normalize_unit(product.unit_of_measure)
        quantity = product.quantity

        if isinstance(quantity, str):
            return product

        factor, normalized_unit = self.__CONVERSIONS.get(unit, (1, unit))
        return Product(
            name=product.name,
            price=product.price,
            quantity=quantity * factor,
            brand=product.brand,
            image_url=product.image_url,
            unit_of_measure=normalized_unit,
        )

    def __normalize_unit(self, unit_of_measure: str) -> str:
        unit = unit_of_measure.strip().lower()
        return self.__UNIT_ALIASES.get(unit, unit)
