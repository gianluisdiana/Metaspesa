from __future__ import annotations

from abc import abstractmethod
from types import TracebackType

from selenium.webdriver.firefox.options import Options
from selenium.webdriver.firefox.webdriver import WebDriver

from domain import Market, Product


class Scrapper:
    def __init__(self, market: Market):
        self.market = market
        options = Options()
        options.add_argument("--headless")
        self.driver = WebDriver(options=options)

    @abstractmethod
    def scrape(self, postal_code: str) -> list[Product]:
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
