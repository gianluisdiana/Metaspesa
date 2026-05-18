import re
from typing import override

from application.product_processors.base import ProductProcessor
from domain import Product


class QuantityRedundancyRemover(ProductProcessor):
    def __init__(self) -> None:
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
            image_url=product.image_url,
        )
