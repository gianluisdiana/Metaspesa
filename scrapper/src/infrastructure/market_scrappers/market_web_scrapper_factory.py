from application.abstractions import MarketWebScrapper
from config import AppConfig
from infrastructure.market_scrappers.alcampo_web_scrapper import AlcampoWebScrapper
from infrastructure.market_scrappers.mercadona_web_scrapper import MercadonaWebScrapper
from infrastructure.playwright_driver import PlaywrightDriver


class MarketWebScrapperFactory:
    def __init__(self, settings: AppConfig, web_driver: PlaywrightDriver):
        self.__settings = settings
        self.__web_driver = web_driver

    def create(self, market_name: str) -> MarketWebScrapper:
        if market_name == "Mercadona":
            return MercadonaWebScrapper(self.__web_driver)
        if market_name == "Alcampo":
            return AlcampoWebScrapper(self.__web_driver, self.__settings.scrapers)
        raise ValueError(f"Unsupported market: {market_name}")
