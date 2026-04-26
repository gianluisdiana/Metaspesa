from abc import ABC, abstractmethod

from domain import Product, Subcategory


class MarketWebScraper(ABC):
    @abstractmethod
    async def set_location(self, postal_code: str) -> None:
        pass

    @abstractmethod
    async def get_categories(self) -> list[str]:
        pass

    @abstractmethod
    async def get_subcategories(self, category: str) -> list[Subcategory]:
        pass

    @abstractmethod
    async def scrape_subcategory(self, subcategory: Subcategory) -> list[Product]:
        pass


class ProductRepository(ABC):
    @abstractmethod
    async def save(self, market_name: str, products: list[Product]) -> None:
        pass
