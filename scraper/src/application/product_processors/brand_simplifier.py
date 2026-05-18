import re
from typing import override

from application.product_processors.base import ProductProcessor
from domain import Product


class BrandSimplifier(ProductProcessor):
    def __init__(self, replacements: dict[str, list[str]]) -> None:
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

                return Product(
                    name=simplified_name,
                    price=product.price,
                    quantity=product.quantity,
                    brand=product.brand,
                    image_url=product.image_url,
                )

        return product
