from application.abstractions import MarketWebScraper, ProductRepository
from application.product_processors import ProductProcessor
from domain import Product


class ScrapeMarketsCommandHandler:
    def __init__(
        self,
        product_repository: ProductRepository,
        market_web_scrapers: dict[str, MarketWebScraper],
        product_processor: ProductProcessor,
    ) -> None:
        self.__product_repository = product_repository
        self.__market_web_scrapers = market_web_scrapers
        self.__product_processor = product_processor

    async def handle(self, postal_code: str) -> None:
        assert len(self.__market_web_scrapers) > 0

        for market_name in self.__market_web_scrapers:
            scraper = self.__market_web_scrapers[market_name]
            products = await self.__scrape(scraper, postal_code)
            products = [self.__product_processor.process(p) for p in products]
            await self.__product_repository.save(market_name, products)

    async def __scrape(
        self, scraper: MarketWebScraper, postal_code: str
    ) -> list[Product]:
        await scraper.set_location(postal_code)
        categories = await scraper.get_categories()
        products: list[Product] = []
        for category in categories:
            subcategories = await scraper.get_subcategories(category)
            for subcategory in subcategories:
                products += await scraper.scrape_subcategory(subcategory)
        return products
