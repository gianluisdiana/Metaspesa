from datetime import datetime

from domain import Market, Product
from market_scrappers.scrapper_factory import ScrapperFactory


def main() -> None:
    markets: list[Market] = [
        Market(name="Mercadona", url="https://www.mercadona.es/"),
    ]

    today = datetime.now().strftime("%Y-%m-%d")
    postal_code = "38320"

    for market in markets:
        products: list[Product]
        with ScrapperFactory().create(market) as scrapper:
            products = scrapper.scrape(postal_code)

        with open(
            f"data/{market.name.lower()}_{today}.csv", "w", encoding="utf-8"
        ) as f:
            f.write("Name;Price;Quantity\n")
            for product in products:
                f.write(f"{product.name};{product.price} €;{product.quantity}\n")


if __name__ == "__main__":
    main()
