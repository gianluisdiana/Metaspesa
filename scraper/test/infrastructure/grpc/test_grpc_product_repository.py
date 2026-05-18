from datetime import UTC, date, datetime, timedelta
from types import SimpleNamespace

from domain import Product
from infrastructure.grpc.grpc_product_repository import (
    AddProductsRequestMapper,
    GrpcProductRepository,
)


class FakeMarketStub:
    def __init__(self) -> None:
        self.add_products_calls: list[tuple[object, object]] = []

    async def AddProducts(self, request, metadata=None) -> None:
        self.add_products_calls.append((request, metadata))


class FakeAuthStub:
    def __init__(self, expirations: list[datetime] | None = None) -> None:
        self.login_requests: list[object] = []
        self.__expirations = expirations or [datetime.now(UTC) + timedelta(hours=1)]

    async def Login(self, request):
        self.login_requests.append(request)
        expiration = self.__expirations.pop(0)
        return SimpleNamespace(
            token=f"token-{len(self.login_requests)}",
            expiration_in_utc=expiration.isoformat(),
        )


def branded_product(name: str = "Product") -> Product:
    return Product(
        name=name,
        price=1.99,
        quantity="500g",
        brand="Brand",
        image_url="https://example.com/product.png",
    )


def brandless_product() -> Product:
    return Product(
        name="Brandless",
        price=2.99,
        quantity="1kg",
        brand=None,
        image_url="https://example.com/brandless.png",
    )


def make_repository(
    market_stub: FakeMarketStub,
    auth_stub: FakeAuthStub,
) -> GrpcProductRepository:
    return GrpcProductRepository(
        channel=None,  # type: ignore[arg-type]
        username="scraper",
        password="password",
        market_stub=market_stub,
        auth_stub=auth_stub,
    )


def authorization(metadata) -> str:
    return metadata.get("authorization")


def mapped_request():
    # Arrange
    mapper = AddProductsRequestMapper()
    registered_at = date(2026, 5, 18)

    # Act
    return mapper.to_request("Market", registered_at, [branded_product()])


def test_maps_one_product_to_add_products_request():
    # Arrange / Act
    request = mapped_request()

    # Assert
    assert len(request.products) == 1


def test_maps_product_name_to_add_products_request():
    # Arrange / Act
    product = mapped_request().products[0]

    # Assert
    assert product.name == "Product"


def test_maps_product_price_to_add_products_request():
    # Arrange / Act
    product = mapped_request().products[0]

    # Assert
    assert abs(product.price - 1.99) < 0.001


def test_maps_product_quantity_to_add_products_request():
    # Arrange / Act
    product = mapped_request().products[0]

    # Assert
    assert product.quantity == "500g"


def test_maps_market_name_to_add_products_request():
    # Arrange / Act
    product = mapped_request().products[0]

    # Assert
    assert product.market_name == "Market"


def test_maps_product_brand_to_add_products_request():
    # Arrange / Act
    product = mapped_request().products[0]

    # Assert
    assert product.brand_name == "Brand"


def test_maps_product_image_url_to_add_products_request():
    # Arrange / Act
    product = mapped_request().products[0]

    # Assert
    assert product.image_url == "https://example.com/product.png"


def test_maps_registered_at_to_add_products_request():
    # Arrange / Act
    request = mapped_request()

    # Assert
    assert request.registered_at.ToDatetime(tzinfo=UTC) == datetime(
        2026, 5, 18, tzinfo=UTC
    )


def test_filters_products_without_brand_from_request():
    # Arrange
    mapper = AddProductsRequestMapper()

    # Act
    request = mapper.to_request(
        "Market", date(2026, 5, 18), [branded_product(), brandless_product()]
    )

    # Assert
    assert [product.name for product in request.products] == ["Product"]


async def test_authenticates_once_before_save():
    # Arrange
    market_stub = FakeMarketStub()
    auth_stub = FakeAuthStub()
    repository = make_repository(market_stub, auth_stub)

    # Act
    await repository.save("Market", date(2026, 5, 18), [branded_product()])

    # Assert
    assert len(auth_stub.login_requests) == 1


async def test_authenticates_with_username_before_save():
    # Arrange
    market_stub = FakeMarketStub()
    auth_stub = FakeAuthStub()
    repository = make_repository(market_stub, auth_stub)

    # Act
    await repository.save("Market", date(2026, 5, 18), [branded_product()])

    # Assert
    assert auth_stub.login_requests[0].username == "scraper"


async def test_authenticates_with_password_before_save():
    # Arrange
    market_stub = FakeMarketStub()
    auth_stub = FakeAuthStub()
    repository = make_repository(market_stub, auth_stub)

    # Act
    await repository.save("Market", date(2026, 5, 18), [branded_product()])

    # Assert
    assert auth_stub.login_requests[0].password == "password"


async def test_sends_authorization_metadata_after_authentication():
    # Arrange
    market_stub = FakeMarketStub()
    auth_stub = FakeAuthStub()
    repository = make_repository(market_stub, auth_stub)

    # Act
    await repository.save("Market", date(2026, 5, 18), [branded_product()])

    # Assert
    _, metadata = market_stub.add_products_calls[0]
    assert authorization(metadata) == "Bearer token-1"


async def test_reuses_valid_token_without_logging_in_again():
    # Arrange
    market_stub = FakeMarketStub()
    auth_stub = FakeAuthStub()
    repository = make_repository(market_stub, auth_stub)

    # Act
    await repository.save("Market", date(2026, 5, 18), [branded_product("Product 1")])
    await repository.save("Market", date(2026, 5, 19), [branded_product("Product 2")])

    # Assert
    assert len(auth_stub.login_requests) == 1


async def test_saves_both_requests_when_reusing_valid_token():
    # Arrange
    market_stub = FakeMarketStub()
    auth_stub = FakeAuthStub()
    repository = make_repository(market_stub, auth_stub)

    # Act
    await repository.save("Market", date(2026, 5, 18), [branded_product("Product 1")])
    await repository.save("Market", date(2026, 5, 19), [branded_product("Product 2")])

    # Assert
    assert len(market_stub.add_products_calls) == 2


async def test_refreshes_expired_token_by_logging_in_again():
    # Arrange
    market_stub = FakeMarketStub()
    auth_stub = FakeAuthStub(
        [
            datetime.now(UTC) - timedelta(minutes=1),
            datetime.now(UTC) + timedelta(hours=1),
        ]
    )
    repository = make_repository(market_stub, auth_stub)

    # Act
    await repository.save("Market", date(2026, 5, 18), [branded_product("Product 1")])
    await repository.save("Market", date(2026, 5, 19), [branded_product("Product 2")])

    # Assert
    assert len(auth_stub.login_requests) == 2


async def test_uses_first_token_for_first_save_when_token_expires():
    # Arrange
    market_stub = FakeMarketStub()
    auth_stub = FakeAuthStub(
        [
            datetime.now(UTC) - timedelta(minutes=1),
            datetime.now(UTC) + timedelta(hours=1),
        ]
    )
    repository = make_repository(market_stub, auth_stub)

    # Act
    await repository.save("Market", date(2026, 5, 18), [branded_product("Product 1")])
    await repository.save("Market", date(2026, 5, 19), [branded_product("Product 2")])

    # Assert
    _, first_metadata = market_stub.add_products_calls[0]
    assert authorization(first_metadata) == "Bearer token-1"


async def test_uses_refreshed_token_for_second_save_when_token_expires():
    # Arrange
    market_stub = FakeMarketStub()
    auth_stub = FakeAuthStub(
        [
            datetime.now(UTC) - timedelta(minutes=1),
            datetime.now(UTC) + timedelta(hours=1),
        ]
    )
    repository = make_repository(market_stub, auth_stub)

    # Act
    await repository.save("Market", date(2026, 5, 18), [branded_product("Product 1")])
    await repository.save("Market", date(2026, 5, 19), [branded_product("Product 2")])

    # Assert
    _, second_metadata = market_stub.add_products_calls[1]
    assert authorization(second_metadata) == "Bearer token-2"
