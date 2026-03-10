from abc import abstractmethod

from selenium.webdriver.firefox.webdriver import WebDriver

from domain import Market, Product

class Scrapper:
    def __init__(self, market: Market):
        self.market = market
        self.driver = WebDriver()

    @abstractmethod
    def scrape(self, postal_code: str) -> list[Product]:
        pass