from __future__ import annotations

import logging
import re
from abc import abstractmethod
from typing import override

from domain import Product


class ProductProcessor:
    def __init__(self):
        self.__next: ProductProcessor | None = None

    def next(self, next_processor: ProductProcessor) -> ProductProcessor:
        self.__next = next_processor
        return next_processor

    def process(self, product: Product) -> Product:
        processed_product = self._process(product)
        if self.__next is not None:
            return self.__next.process(processed_product)
        return processed_product

    @abstractmethod
    def _process(self, product: Product) -> Product:
        pass


class BrandExtractor(ProductProcessor):
    def __init__(self, known_brands: list[str]):
        super().__init__()
        self.__logger = logging.getLogger("BrandExtractor")
        self.__known_brands = sorted(known_brands, key=lambda x: len(x), reverse=True)

    @override
    def _process(self, product: Product) -> Product:
        if product.brand is not None:
            return product

        for brand in self.__known_brands:
            pattern = rf"\b{re.escape(brand)}\b"
            name_without_brand = re.sub(
                pattern, "", product.name, flags=re.IGNORECASE
            ).strip()

            if name_without_brand != product.name:
                product_with_brand = Product(
                    name=name_without_brand,
                    price=product.price,
                    quantity=product.quantity,
                    brand=brand,
                )

                return product_with_brand

        self.__logger.warning("Could not extract brand for product: '%s'", product.name)
        return product


class QuantityRedundancyRemover(ProductProcessor):
    def __init__(self):
        super().__init__()
        packaging_prefix_pattern = r"(?:(?:tarrina|malla|botella|bolsa|bandeja) (?:de )?|(?:pack (?:de )?)?(?:\d+ )(?:uds?\. ?)?x ?)?"  # noqa: E501
        amount_pattern = r"\d+(?:[.,]\d+)?"
        unit_pattern = r" ?(?:(?:(?:k(?:ilo)?|c(?:enti)?|m)?(?:l(?:itros?)?|g(?:r(?:amos?)?)?)(?: aproximadamente)?|uds?|ds)(?:[^\w\s]|$)|lav(?:ados)?)"  # noqa: E501
        simple_packaging_pattern = r"bandeja|al peso"
        self.__quantity_pattern = re.compile(
            rf",? (?:{packaging_prefix_pattern}{amount_pattern}{unit_pattern}|{simple_packaging_pattern})",  # noqa: E501
            re.IGNORECASE,
        )

    @override
    def _process(self, product: Product) -> Product:
        if not self.__quantity_pattern.search(product.name):
            return product

        name = self.__quantity_pattern.sub("", product.name).strip()

        return Product(
            name=name,
            price=product.price,
            quantity=product.quantity,
            brand=product.brand,
        )


class BrandSimplifier(ProductProcessor):
    def __init__(self, replacements: dict[str, list[str]]):
        super().__init__()
        self.__replacements: dict[str, list[str]] = replacements

    @override
    def _process(self, product: Product) -> Product:
        if product.brand is not None:
            return product

        for standard_brand, variants in self.__replacements.items():
            if any(variant.lower() in product.name.lower() for variant in variants):
                simplified_name = product.name
                for variant in variants:
                    pattern = rf"\b{re.escape(variant)}\b"
                    simplified_name = re.sub(
                        pattern, standard_brand, simplified_name, flags=re.IGNORECASE
                    )

                simplified_product = Product(
                    name=simplified_name,
                    price=product.price,
                    quantity=product.quantity,
                    brand=None,
                )
                return simplified_product

        return product
