from abc import ABC, abstractmethod
from datetime import date

from domain import Product, Subcategory


class MarketWebScraper(ABC):
    @abstractmethod
    async def set_location(self, postal_code: str) -> None: ...

    @abstractmethod
    async def get_categories(self) -> list[str]: ...

    @abstractmethod
    async def get_subcategories(self, category: str) -> list[Subcategory]: ...

    @abstractmethod
    async def scrape_subcategory(self, subcategory: Subcategory) -> list[Product]: ...


class ProductRepository(ABC):
    @abstractmethod
    async def save(
        self, market_name: str, date: date, products: list[Product]
    ) -> None: ...


class FallbackProductRepository(ProductRepository):
    @abstractmethod
    async def get_products_by_market_and_date(
        self, market_name: str, date: date
    ) -> list[Product]: ...

    @abstractmethod
    async def get_markets_and_dates(self) -> list[tuple[str, date]]: ...

    @abstractmethod
    async def remove_old_products(self, market_name: str, date: date) -> None: ...


class RepositorySaveException(Exception):
    pass
