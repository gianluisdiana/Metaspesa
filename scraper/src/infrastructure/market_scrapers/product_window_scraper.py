from collections.abc import Awaitable, Callable
from logging import Logger

from bs4 import BeautifulSoup

from domain import Product
from infrastructure.market_scrapers.product_scroll import ProductScrollScraper
from infrastructure.market_scrapers.product_tags import ProductTag, ProductTagCollection
from infrastructure.market_scrapers.resilience import RetryPolicy


class ProductWindowScraper:
    def __init__(self, retry_policy: RetryPolicy, logger: Logger) -> None:
        self.__product_scroll_scraper = ProductScrollScraper(retry_policy, logger)

    async def scrape(
        self,
        *,
        get_page_source: Callable[[], Awaitable[str]],
        parse_tags: Callable[[BeautifulSoup], list[ProductTag]],
        scroll: Callable[[], Awaitable[None]],
        recover_after_failed_extraction: Callable[[], Awaitable[None]] | None = None,
    ) -> list[Product]:
        async def get_products_from_current_window(
            products: list[Product],
        ) -> tuple[list[Product], list[ProductTag]]:
            soup = BeautifulSoup(await get_page_source(), "html.parser")

            product_tags = ProductTagCollection(parse_tags(soup))

            last_added_product = products[-1] if products else None
            products += product_tags.new_products_since(last_added_product)

            return products, product_tags.tags

        return await self.__product_scroll_scraper.scrape(
            get_products_from_current_window=get_products_from_current_window,
            scroll=scroll,
            recover_after_failed_extraction=recover_after_failed_extraction,
        )
