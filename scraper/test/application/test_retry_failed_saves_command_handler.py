from datetime import date

from conftest import (
    DummyFallbackRepository,
    DummyProductRepository,
    FailingProductRepository,
    SpyFallbackRepository,
    SpyProductRepository,
)

from application.use_case import RetryFailedSavesCommandHandler
from domain import Product


def make_handler(**kwargs) -> RetryFailedSavesCommandHandler:
    defaults: dict = dict(
        fallback_repository=DummyFallbackRepository(),
        main_repository=DummyProductRepository(),
    )
    defaults.update(kwargs)
    return RetryFailedSavesCommandHandler(**defaults)


async def test_does_nothing_if_no_markets_and_dates():
    # Arrange
    fallback = SpyFallbackRepository(markets_and_dates=[])
    main = SpyProductRepository()
    handler = make_handler(fallback_repository=fallback, main_repository=main)

    # Act
    await handler.handle()

    # Assert
    assert main.save_calls == []


async def test_saves_products_to_main_repository():
    # Arrange
    today = date.today()
    products = [Product(name="product1", price=1.0, quantity="1 unit")]
    fallback = SpyFallbackRepository(
        markets_and_dates=[("Market", today)], products=products
    )
    main = SpyProductRepository()
    handler = make_handler(fallback_repository=fallback, main_repository=main)

    # Act
    await handler.handle()

    # Assert
    assert main.saved_products == products


async def test_saves_with_correct_market_name_and_date():
    # Arrange
    today = date.today()
    fallback = SpyFallbackRepository(
        markets_and_dates=[("Mercadona", today)], products=[]
    )
    main = SpyProductRepository()
    handler = make_handler(fallback_repository=fallback, main_repository=main)

    # Act
    await handler.handle()

    # Assert
    market_name, saved_date, _ = main.save_calls[0]
    assert market_name == "Mercadona"
    assert saved_date == today


async def test_removes_old_products_from_fallback_on_success():
    # Arrange
    today = date.today()
    fallback = SpyFallbackRepository(markets_and_dates=[("Market", today)])
    handler = make_handler(fallback_repository=fallback)

    # Act
    await handler.handle()

    # Assert
    assert fallback.remove_calls == [("Market", today)]


async def test_does_not_remove_old_products_if_save_fails():
    # Arrange
    today = date.today()
    fallback = SpyFallbackRepository(markets_and_dates=[("Market", today)])
    handler = make_handler(
        fallback_repository=fallback,
        main_repository=FailingProductRepository(),
    )

    # Act
    await handler.handle()

    # Assert
    assert fallback.remove_calls == []


async def test_handles_multiple_markets():
    # Arrange
    today = date.today()
    markets_and_dates = [("Market1", today), ("Market2", today)]
    fallback = SpyFallbackRepository(markets_and_dates=markets_and_dates)
    main = SpyProductRepository()
    handler = make_handler(fallback_repository=fallback, main_repository=main)

    # Act
    await handler.handle()

    # Assert
    assert len(main.save_calls) == 2
    saved_names = [call[0] for call in main.save_calls]
    assert "Market1" in saved_names
    assert "Market2" in saved_names
