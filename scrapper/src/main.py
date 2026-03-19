import logging
from datetime import datetime

from domain import Market, Product
from market_scrappers.scrapper_factory import ScrapperFactory

logging.basicConfig(
    level=logging.INFO,
    format="\033[1m%(asctime)s\033[0m - \033[1m%(levelname)s\033[0m: %(name)s\n\t%(message)s",  # noqa: E501
)


def main() -> None:
    markets: list[Market] = [
        Market(name="Alcampo", url="https://www.compraonline.alcampo.es/"),
        Market(name="Mercadona", url="https://www.mercadona.es/"),
    ]

    today = datetime.now().strftime("%Y-%m-%d")
    postal_code = "38320"

    for market in markets:
        products: list[Product]
        with ScrapperFactory().create(market) as scrapper:
            products = scrapper.scrape(postal_code)

        with open(
            f"data/{today}_{market.name.lower()}.csv", "w", encoding="utf-8"
        ) as f:
            f.write("Name;Price;Quantity\n")
            for product in products:
                f.write(f'"{product.name}";{product.price};{product.quantity}\n')


if __name__ == "__main__":
    main()
