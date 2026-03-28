import logging

from selenium.webdriver.remote.webdriver import WebDriver

from config import AppConfig
from market_scrappers.alcampo_web_scrapper import AlcampoWebScrapper
from market_scrappers.mercadona_web_scrapper import MercadonaWebScrapper
from market_scrappers.scrapper import Scrapper


class ScrapperFactory:
    def __init__(self, settings: AppConfig):
        self.__settings = settings

    def create(self, market_name: str, web_driver: WebDriver) -> Scrapper:
        if market_name not in ["Mercadona", "Alcampo"]:
            raise ValueError(f"Unsupported market: {market_name}")

        if market_name == "Mercadona":
            logger = logging.getLogger("MercadonaWebScrapper")
            web_scrapper = MercadonaWebScrapper(web_driver)
        else:
            logger = logging.getLogger("AlcampoWebScrapper")
            web_scrapper = AlcampoWebScrapper(
                web_driver, self.__settings.scrapers.alcampo
            )
        return Scrapper(web_scrapper=web_scrapper, logger=logger)
