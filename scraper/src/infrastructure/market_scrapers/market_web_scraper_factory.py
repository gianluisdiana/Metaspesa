from application.abstractions import MarketWebScraper
from config import AppConfig
from infrastructure.market_scrapers.alcampo_web_scraper import AlcampoWebScraper
from infrastructure.market_scrapers.mercadona_web_scraper import MercadonaWebScraper
from infrastructure.playwright_driver import PlaywrightDriver


class MarketWebScraperFactory:
    def __init__(self, settings: AppConfig, web_driver: PlaywrightDriver):
        self.__settings = settings
        self.__web_driver = web_driver

    def create(self, market_name: str) -> MarketWebScraper:
        if market_name == "Mercadona":
            return MercadonaWebScraper(self.__web_driver)
        if market_name == "Alcampo":
            return AlcampoWebScraper(self.__web_driver, self.__settings.scrapers)
        raise ValueError(f"Unsupported market: {market_name}")
