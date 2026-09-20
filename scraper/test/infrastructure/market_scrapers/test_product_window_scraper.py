import logging

from bs4 import BeautifulSoup, Tag

from domain import Product
from infrastructure.market_scrapers.product_tags import ProductTag
from infrastructure.market_scrapers.product_window_scraper import ProductWindowScraper
from infrastructure.market_scrapers.resilience import RetryPolicy


class PageProductTag:
    def __init__(self, tag: Tag) -> None:
        self.tag = tag

    def is_ready(self) -> bool:
        return self.tag.get("data-ready") == "true"

    def to_product(self) -> Product:
        name = str(self.tag["data-name"])
        return Product(
            raw_content=name,
            name=name,
            price=1.0,
            quantity="1 unit",
            image_url="https://example.com/product.png",
        )


def parse_tags(soup: BeautifulSoup) -> list[ProductTag]:
    return [PageProductTag(tag) for tag in soup.select("li[data-name]")]


def product_page(ready_count: int) -> str:
    tags = "".join(
        f'<li data-name="Product {index}" '
        f'data-ready="{str(index <= ready_count).lower()}"></li>'
        for index in range(1, 4)
    )
    return f"<ul>{tags}</ul>"


async def test_returns_products_loaded_before_scroll_progress_stops() -> None:
    logger = logging.getLogger("test_product_window_scraper")

    async def get_page_source() -> str:
        return product_page(1)

    async def scroll() -> None:
        pass

    products = await ProductWindowScraper(RetryPolicy(), logger).scrape(
        get_page_source=get_page_source,
        parse_tags=parse_tags,
        scroll=scroll,
    )

    assert [product.name for product in products] == ["Product 1"]


async def test_stops_after_five_scrolls_without_new_products() -> None:
    logger = logging.getLogger("test_product_window_scraper")
    scroll_count = 0

    async def get_page_source() -> str:
        return product_page(1)

    async def scroll() -> None:
        nonlocal scroll_count
        scroll_count += 1

    await ProductWindowScraper(RetryPolicy(), logger).scrape(
        get_page_source=get_page_source,
        parse_tags=parse_tags,
        scroll=scroll,
    )

    assert scroll_count == 5


async def test_logs_missing_products_after_scroll_progress_stops(caplog) -> None:
    logger = logging.getLogger("test_product_window_scraper")

    async def get_page_source() -> str:
        return product_page(1)

    async def scroll() -> None:
        pass

    with caplog.at_level(logging.ERROR, logger=logger.name):
        await ProductWindowScraper(RetryPolicy(), logger).scrape(
            get_page_source=get_page_source,
            parse_tags=parse_tags,
            scroll=scroll,
        )

    assert "Could not scrape 2 products after 5 consecutive scrolls" in caplog.text


async def test_new_products_reset_consecutive_scroll_count() -> None:
    logger = logging.getLogger("test_product_window_scraper")
    ready_counts = iter([1, 1, 2, 2, 2, 3])

    async def get_page_source() -> str:
        return product_page(next(ready_counts))

    async def scroll() -> None:
        pass

    products = await ProductWindowScraper(RetryPolicy(), logger).scrape(
        get_page_source=get_page_source,
        parse_tags=parse_tags,
        scroll=scroll,
    )

    assert [product.name for product in products] == [
        "Product 1",
        "Product 2",
        "Product 3",
    ]


async def test_does_not_log_missing_products_when_all_products_load(caplog) -> None:
    logger = logging.getLogger("test_product_window_scraper")
    ready_counts = iter([1, 1, 2, 2, 2, 3])

    async def get_page_source() -> str:
        return product_page(next(ready_counts))

    async def scroll() -> None:
        pass

    with caplog.at_level(logging.ERROR, logger=logger.name):
        await ProductWindowScraper(RetryPolicy(), logger).scrape(
            get_page_source=get_page_source,
            parse_tags=parse_tags,
            scroll=scroll,
        )

    assert "Could not scrape" not in caplog.text


async def test_logs_product_loading_progress(caplog) -> None:
    logger = logging.getLogger("test_product_window_scraper")

    async def get_page_source() -> str:
        return product_page(1)

    async def scroll() -> None:
        pass

    with caplog.at_level(logging.DEBUG, logger=logger.name):
        await ProductWindowScraper(RetryPolicy(), logger).scrape(
            get_page_source=get_page_source,
            parse_tags=parse_tags,
            scroll=scroll,
        )

    assert (
        "Loaded 1 / 3 products [ 33% ######--------------], scrolling for more..."
        in caplog.text
    )


async def test_does_not_scroll_when_all_products_are_ready() -> None:
    logger = logging.getLogger("test_product_window_scraper")
    scroll_count = 0

    async def get_page_source() -> str:
        return product_page(3)

    async def scroll() -> None:
        nonlocal scroll_count
        scroll_count += 1

    await ProductWindowScraper(RetryPolicy(), logger).scrape(
        get_page_source=get_page_source,
        parse_tags=parse_tags,
        scroll=scroll,
    )

    assert scroll_count == 0
