import unicodedata
from typing import override

from application.product_processors.base import ProductProcessor
from domain import Product


class StringSanitizer(ProductProcessor):
    @override
    def _process(self, product: Product) -> Product:
        return Product(
            name=self.__sanitize(product.name),
            price=product.price,
            quantity=self.__sanitize(product.quantity),
            brand=self.__sanitize(product.brand) if product.brand is not None else None,
            image_url=self.__sanitize(product.image_url),
        )

    @staticmethod
    def __sanitize(value: str) -> str:
        if value.isascii():
            return value

        normalized_value = unicodedata.normalize("NFD", value)
        return "".join(
            character
            for character in normalized_value
            if unicodedata.category(character) not in {"Mn", "Mc", "Me"}
            and character.isascii()
            and character.isprintable()
        )
