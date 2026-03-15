import logging

from domain import Market
from market_scrappers.alcampo_scrapper import AlcampoScrapper
from market_scrappers.mercadona_scrapper import MercadonaScrapper
from market_scrappers.scrapper import Scrapper


class ScrapperFactory:
    def create(self, market: Market) -> Scrapper:
        logger = logging.getLogger(f"{market.name}Scrapper")
        if market.name == "Mercadona":
            return MercadonaScrapper(market, logger)
        if market.name == "Alcampo":
            return AlcampoScrapper(market, logger)
        raise ValueError(f"Unsupported market: {market.name}")
