from config import AppConfig
from market_scrappers.alcampo_scrapper import AlcampoScrapper
from market_scrappers.mercadona_scrapper import MercadonaScrapper
from market_scrappers.scrapper import Scrapper


class ScrapperFactory:
    def __init__(self, settings: AppConfig):
        self.__settings = settings

    def create(self, market_name: str) -> Scrapper:
        if market_name == "Mercadona":
            return MercadonaScrapper()
        if market_name == "Alcampo":
            return AlcampoScrapper(self.__settings.scrapers.alcampo)
        raise ValueError(f"Unsupported market: {market_name}")
