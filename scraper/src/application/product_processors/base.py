from __future__ import annotations

from abc import abstractmethod

from domain import Product


class ProductProcessor:
    def __init__(self) -> None:
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
