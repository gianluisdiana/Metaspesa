from domain import Market
from market_scrappers.mercadona_scrapper import MercadonaScrapper
from market_scrappers.scrapper import Scrapper

class ScrapperFactory:
    def create(self, market: Market) -> Scrapper:
        if market.name == 'Mercadona':
            return MercadonaScrapper(market)
        raise ValueError(f'Unsupported market: {market.name}')