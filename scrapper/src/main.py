import logging
from datetime import datetime

from config import AppConfig, load_config
from domain import Product
from market_scrappers.scrapper_factory import ScrapperFactory

logging.basicConfig(
    level=logging.INFO,
    format="\033[1m%(asctime)s\033[0m - \033[1m%(levelname)s\033[0m: %(name)s\n\t%(message)s",  # noqa: E501
)


def main() -> None:
    settings: AppConfig = load_config()
    market_names: list[str] = [
        "Alcampo",
        "Mercadona",
    ]

    today: str = datetime.now().strftime("%Y-%m-%d")
    postal_code = "38320"

    factory = ScrapperFactory(settings)
    for market_name in market_names:
        with factory.create(market_name) as scrapper:
            products: list[Product] = scrapper.scrape(postal_code)

        with open(
            f"data/{today}_{market_name.lower()}.csv", "w", encoding="utf-8"
        ) as f:
            f.write("Name;Price;Quantity;Brand\n")
            for product in products:
                f.write(
                    f'"{product.name}";{product.price};{product.quantity};{product.brand}\n'
                )


if __name__ == "__main__":
    main()
