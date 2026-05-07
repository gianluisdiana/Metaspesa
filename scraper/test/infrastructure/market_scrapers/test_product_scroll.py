import logging

from infrastructure.market_scrapers.product_scroll import (
    ProductScrollScraper,
)
from infrastructure.market_scrapers.resilience import RetryPolicy


async def test_stops_after_five_consecutive_scrolls_without_new_products(caplog):
    # Arrange
    logger = logging.getLogger("test_product_scroll")
    scroll_count = 0

    async def get_products_from_current_window(
        products: list[str],
    ) -> tuple[list[str], list[str]]:
        if not products:
            products.append("Product 1")
        return products, ["Product 1", "Product 2", "Product 3"]

    async def scroll() -> None:
        nonlocal scroll_count
        scroll_count += 1

    # Act
    with caplog.at_level(logging.WARNING, logger=logger.name):
        products = await ProductScrollScraper(RetryPolicy(), logger).scrape(
            get_products_from_current_window=get_products_from_current_window,
            scroll=scroll,
        )

    # Assert
    assert products == ["Product 1"]


async def test_scrolls_five_times_before_stopping_without_new_products(caplog):
    # Arrange
    logger = logging.getLogger("test_product_scroll")
    scroll_count = 0

    async def get_products_from_current_window(
        products: list[str],
    ) -> tuple[list[str], list[str]]:
        if not products:
            products.append("Product 1")
        return products, ["Product 1", "Product 2", "Product 3"]

    async def scroll() -> None:
        nonlocal scroll_count
        scroll_count += 1

    # Act
    with caplog.at_level(logging.WARNING, logger=logger.name):
        await ProductScrollScraper(RetryPolicy(), logger).scrape(
            get_products_from_current_window=get_products_from_current_window,
            scroll=scroll,
        )

    # Assert
    assert scroll_count == 5


async def test_logs_warning_when_stopping_without_new_products(caplog):
    # Arrange
    logger = logging.getLogger("test_product_scroll")
    scroll_count = 0

    async def get_products_from_current_window(
        products: list[str],
    ) -> tuple[list[str], list[str]]:
        if not products:
            products.append("Product 1")
        return products, ["Product 1", "Product 2", "Product 3"]

    async def scroll() -> None:
        nonlocal scroll_count
        scroll_count += 1

    # Act
    with caplog.at_level(logging.WARNING, logger=logger.name):
        await ProductScrollScraper(RetryPolicy(), logger).scrape(
            get_products_from_current_window=get_products_from_current_window,
            scroll=scroll,
        )

    # Assert
    assert "Could not scrape 2 products after 5 consecutive scrolls" in caplog.text


async def test_resets_consecutive_scroll_count_when_new_products_are_found(caplog):
    # Arrange
    logger = logging.getLogger("test_product_scroll")
    visible_product_counts = [1, 1, 2, 2, 2, 3]

    async def get_products_from_current_window(
        products: list[str],
    ) -> tuple[list[str], list[str]]:
        visible_product_count = visible_product_counts.pop(0)
        products[:] = [
            f"Product {product_number}"
            for product_number in range(1, visible_product_count + 1)
        ]
        return products, ["Product 1", "Product 2", "Product 3"]

    async def scroll() -> None:
        pass

    # Act
    with caplog.at_level(logging.WARNING, logger=logger.name):
        products = await ProductScrollScraper(RetryPolicy(), logger).scrape(
            get_products_from_current_window=get_products_from_current_window,
            scroll=scroll,
        )

    # Assert
    assert products == ["Product 1", "Product 2", "Product 3"]


async def test_does_not_log_warning_when_new_products_are_found(caplog):
    # Arrange
    logger = logging.getLogger("test_product_scroll")
    visible_product_counts = [1, 1, 2, 2, 2, 3]

    async def get_products_from_current_window(
        products: list[str],
    ) -> tuple[list[str], list[str]]:
        visible_product_count = visible_product_counts.pop(0)
        products[:] = [
            f"Product {product_number}"
            for product_number in range(1, visible_product_count + 1)
        ]
        return products, ["Product 1", "Product 2", "Product 3"]

    async def scroll() -> None:
        pass

    # Act
    with caplog.at_level(logging.WARNING, logger=logger.name):
        await ProductScrollScraper(RetryPolicy(), logger).scrape(
            get_products_from_current_window=get_products_from_current_window,
            scroll=scroll,
        )

    # Assert
    assert "Could not scrape" not in caplog.text


async def test_logs_product_loading_progress_bar(caplog):
    # Arrange
    logger = logging.getLogger("test_product_scroll")

    async def get_products_from_current_window(
        products: list[str],
    ) -> tuple[list[str], list[str]]:
        products[:] = ["Product 1"]
        return products, ["Product 1", "Product 2", "Product 3"]

    async def scroll() -> None:
        pass

    # Act
    with caplog.at_level(logging.DEBUG, logger=logger.name):
        await ProductScrollScraper(
            RetryPolicy(), logger, max_scrolls_without_new_products=1
        ).scrape(
            get_products_from_current_window=get_products_from_current_window,
            scroll=scroll,
        )

    # Assert
    assert (
        "Loaded 1 / 3 products [ 33% ######--------------], scrolling for more..."
        in caplog.text
    )
