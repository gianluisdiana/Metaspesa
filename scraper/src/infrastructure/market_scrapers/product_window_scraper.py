from collections.abc import Awaitable, Callable
from logging import Logger

from bs4 import BeautifulSoup

from domain import Product
from infrastructure.market_scrapers.product_tags import ProductTag, ProductTagCollection
from infrastructure.market_scrapers.resilience import (
    SCRAPER_RECOVERABLE_ERRORS,
    RetryPolicy,
)


class ProductWindowScraper:
    def __init__(self, retry_policy: RetryPolicy, logger: Logger) -> None:
        self.__retry_policy = retry_policy
        self.__logger = logger
        self.__max_scrolls_without_new_products = 5

    async def scrape(
        self,
        *,
        get_page_source: Callable[[], Awaitable[str]],
        parse_tags: Callable[[BeautifulSoup], list[ProductTag]],
        scroll: Callable[[], Awaitable[None]],
        recover_after_failed_extraction: Callable[[], Awaitable[None]] | None = None,
    ) -> list[Product]:
        products: list[Product] = []
        scrolls_without_new_products = 0
        has_scrolled = False

        async def get_products_from_current_window() -> tuple[list[Product], int]:
            soup = BeautifulSoup(await get_page_source(), "html.parser")
            product_tags = ProductTagCollection(parse_tags(soup))

            last_added_product = products[-1] if products else None
            products.extend(product_tags.new_products_since(last_added_product))

            return products, len(product_tags.tags)

        while True:
            previous_product_count = len(products)
            loaded_products = await self.__retry_policy.run(
                get_products_from_current_window,
                description="Extracting products from page",
                logger=self.__logger,
                recover=recover_after_failed_extraction,
            )
            if loaded_products is None:
                break

            products, product_tag_count = loaded_products
            if len(products) >= product_tag_count:
                break

            if has_scrolled and len(products) == previous_product_count:
                scrolls_without_new_products += 1
                if (
                    scrolls_without_new_products
                    >= self.__max_scrolls_without_new_products
                ):
                    self.__log_missing_products(product_tag_count, len(products))
                    break
            else:
                scrolls_without_new_products = 0

            self.__log_scroll_progress(len(products), product_tag_count)

            try:
                await scroll()
                has_scrolled = True
            except SCRAPER_RECOVERABLE_ERRORS:
                break

        return products

    def __log_scroll_progress(self, product_count: int, product_tag_count: int) -> None:
        self.__logger.debug(
            "Loaded %d / %d products %s, scrolling for more...",
            product_count,
            product_tag_count,
            self.__progress_bar(product_count, product_tag_count),
            extra={"loaded_percentage": int(product_count / product_tag_count * 100)},
        )

    def __progress_bar(self, product_count: int, product_tag_count: int) -> str:
        bar_width = 20
        percentage = int(product_count / product_tag_count * 100)
        filled_width = int(product_count / product_tag_count * bar_width)
        filled_bar = "#" * filled_width
        empty_bar = "-" * (bar_width - filled_width)
        return f"[{percentage:3d}% {filled_bar}{empty_bar}]"

    def __log_missing_products(
        self, product_tag_count: int, product_count: int
    ) -> None:
        missing_products = max(product_tag_count - product_count, 0)
        self.__logger.error(
            "Could not scrape %d products after %d consecutive scrolls "
            "without new products",
            missing_products,
            self.__max_scrolls_without_new_products,
            extra={
                "loaded_percentage": int(product_count / product_tag_count * 100),
            },
        )
