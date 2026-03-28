from abc import abstractmethod

from domain import Product


class MarketWebScrapper:
    @property
    @abstractmethod
    def url(self) -> str:
        pass

    @abstractmethod
    def navigate_to_home(self) -> None:
        pass

    @abstractmethod
    def close_popups(self) -> None:
        pass

    @abstractmethod
    def set_location(self, postal_code: str) -> None:
        pass

    @abstractmethod
    def navigate_to_categories(self) -> None:
        pass

    @abstractmethod
    def get_categories(self) -> list[str]:
        pass

    @abstractmethod
    def scrape_category(self, category: str) -> list[Product]:
        pass
