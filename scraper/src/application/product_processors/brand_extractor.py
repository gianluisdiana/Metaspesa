import logging
import re
from typing import override

from application.product_processors.base import ProductProcessor
from domain import Product


class BrandExtractor(ProductProcessor):
    def __init__(self, known_brands: list[str]) -> None:
        super().__init__()
        self.__logger = logging.getLogger(self.__class__.__name__)
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
                return Product(
                    name=name_without_brand,
                    price=product.price,
                    quantity=product.quantity,
                    brand=brand,
                    image_url=product.image_url,
                )

        self.__logger.warning(
            "Could not extract brand for product: '%s'",
            product.name,
            extra={"product": product.name},
        )
        return product
