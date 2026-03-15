from __future__ import annotations

from abc import abstractmethod
from logging import Logger
from types import TracebackType

from selenium.webdriver.firefox.options import Options
from selenium.webdriver.firefox.webdriver import WebDriver

from domain import Market, Product


class Scrapper:
    def __init__(self, market: Market, logger: Logger):
        self.market = market
        self.__logger = logger
        options = Options()
        options.add_argument("--headless")
        self.driver = WebDriver(options=options)

    def scrape(self, postal_code: str) -> list[Product]:
        self.driver.get(self.market.url)
        self.__logger.info(f"Navigated to {self.market.url}")

        self._close_popups()
        self.__logger.info("Closed popups")

        self._set_location(postal_code)
        self.__logger.info(f"Location set to postal code {postal_code}")

        self._navigate_to_categories()
        self.__logger.info("Navigated to categories")

        categories = self._get_categories()
        self.__logger.info(f"Found {len(categories)} categories")

        products: list[Product] = []
        for category in categories:
            category_products = self._scrape_category(category)
            products += category_products
            self.__logger.info(
                f"Scraped category '{category}' with {len(category_products)} products"
            )

        return products

    @abstractmethod
    def _set_location(self, postal_code: str) -> None:
        pass

    @abstractmethod
    def _close_popups(self) -> None:
        pass

    @abstractmethod
    def _navigate_to_categories(self) -> None:
        pass

    @abstractmethod
    def _get_categories(self) -> list[str]:
        pass

    @abstractmethod
    def _scrape_category(self, category: str) -> list[Product]:
        pass

    def __enter__(self) -> Scrapper:
        return self

    def __exit__(
        self,
        exc_type: type[BaseException] | None,
        exc_val: BaseException | None,
        exc_tb: TracebackType | None,
    ) -> None:
        self.driver.quit()
