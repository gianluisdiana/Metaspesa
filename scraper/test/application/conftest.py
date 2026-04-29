from datetime import date
from typing import override

from application.abstractions import (
    FallbackProductRepository,
    ProductRepository,
    RepositorySaveException,
)
from domain import Product


class DummyProductRepository(ProductRepository):
    @override
    async def save(self, market_name: str, date: date, products: list[Product]) -> None:
        pass


class SpyProductRepository(ProductRepository):
    def __init__(self):
        self.save_calls: list[tuple[str, date, list[Product]]] = []

    @property
    def saved_products(self) -> list[Product]:
        return self.save_calls[-1][2] if self.save_calls else []

    @override
    async def save(self, market_name: str, date: date, products: list[Product]) -> None:
        self.save_calls.append((market_name, date, products))


class FailingProductRepository(ProductRepository):
    @override
    async def save(self, market_name: str, date: date, products: list[Product]) -> None:
        raise RepositorySaveException("Save failed")


class DummyFallbackRepository(FallbackProductRepository):
    @override
    async def save(self, market_name: str, date: date, products: list[Product]) -> None:
        pass

    @override
    async def get_products_by_market_and_date(
        self, market_name: str, date: date
    ) -> list[Product]:
        return []

    @override
    async def get_markets_and_dates(self) -> list[tuple[str, date]]:
        return []

    @override
    async def remove_old_products(self, market_name: str, date: date) -> None:
        pass


class SpyFallbackRepository(DummyFallbackRepository):
    def __init__(
        self,
        markets_and_dates: list[tuple[str, date]] | None = None,
        products: list[Product] | None = None,
    ):
        self.save_calls: list[tuple[str, date, list[Product]]] = []
        self.remove_calls: list[tuple[str, date]] = []
        self.__markets_and_dates = markets_and_dates or []
        self.__products = products or []

    @override
    async def save(self, market_name: str, date: date, products: list[Product]) -> None:
        self.save_calls.append((market_name, date, products))

    @override
    async def get_markets_and_dates(self) -> list[tuple[str, date]]:
        return self.__markets_and_dates

    @override
    async def get_products_by_market_and_date(
        self, market_name: str, date: date
    ) -> list[Product]:
        return self.__products

    @override
    async def remove_old_products(self, market_name: str, date: date) -> None:
        self.remove_calls.append((market_name, date))
