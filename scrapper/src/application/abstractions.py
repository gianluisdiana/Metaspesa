from abc import abstractmethod

from domain import Product, Subcategory


class MarketWebScrapper:
    @property
    @abstractmethod
    def url(self) -> str:
        pass

    @abstractmethod
    async def navigate_to_home(self) -> None:
        pass

    @abstractmethod
    async def close_popups(self) -> None:
        pass

    @abstractmethod
    async def set_location(self, postal_code: str) -> None:
        pass

    @abstractmethod
    async def navigate_to_categories(self) -> None:
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


class ProductRepository:
    @abstractmethod
    async def save(self, market_name: str, products: list[Product]) -> None:
        pass
