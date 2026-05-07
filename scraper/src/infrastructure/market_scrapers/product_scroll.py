from collections.abc import Awaitable, Callable, Sequence
from logging import Logger

from infrastructure.market_scrapers.resilience import (
    SCRAPER_RECOVERABLE_ERRORS,
    RetryPolicy,
)


class ProductScrollScraper:
    def __init__(
        self,
        retry_policy: RetryPolicy,
        logger: Logger,
        max_scrolls_without_new_products: int = 5,
    ) -> None:
        self.__retry_policy = retry_policy
        self.__logger = logger
        self.__max_scrolls_without_new_products = max_scrolls_without_new_products

    async def scrape[ProductT, ProductTagT](
        self,
        *,
        get_products_from_current_window: Callable[
            [list[ProductT]], Awaitable[tuple[list[ProductT], Sequence[ProductTagT]]]
        ],
        scroll: Callable[[], Awaitable[object]],
        recover_after_failed_extraction: Callable[[], Awaitable[None]] | None = None,
    ) -> list[ProductT]:
        products: list[ProductT] = []
        scrolls_without_new_products = 0
        has_scrolled = False

        while True:
            previous_product_count = len(products)
            loaded_products = await self.__retry_policy.run(
                lambda ps=products: get_products_from_current_window(ps),
                description="Extracting products from page",
                logger=self.__logger,
                recover=recover_after_failed_extraction,
            )
            if loaded_products is None:
                break

            products, product_tags = loaded_products
            if len(products) >= len(product_tags):
                break

            if has_scrolled and len(products) == previous_product_count:
                scrolls_without_new_products += 1
                if (
                    scrolls_without_new_products
                    >= self.__max_scrolls_without_new_products
                ):
                    self.__log_missing_products(len(product_tags), len(products))
                    break
            else:
                scrolls_without_new_products = 0

            self.__log_scroll_progress(len(products), len(product_tags))

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
        )
