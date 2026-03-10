from datetime import datetime

from domain import Market
from market_scrappers.scrapper import Scrapper
from market_scrappers.scrapper_factory import ScrapperFactory

def main() -> None:
    markets: list[Market] = [
        Market(name='Mercadona', url='https://www.mercadona.es/'),
    ]

    scrappers: list[Scrapper] = [
        ScrapperFactory().create(market)
        for market in markets
    ]

    today = datetime.now().strftime('%Y-%m-%d')
    postal_code = '38320'

    for scrapper in scrappers:
        products = scrapper.scrape(postal_code)
        with open(f'{scrapper.market.name.lower()}_{today}.csv', 'w', encoding='utf-8') as f:
            f.write('Name;Price;Quantity\n')
            for product in products:
                f.write(f'{product.name};{product.price} €;{product.quantity}\n')

if __name__ == '__main__':
    main()