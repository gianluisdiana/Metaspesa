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


def test_maps_products_to_add_products_request():
    # Arrange
    mapper = AddProductsRequestMapper()
    registered_at = date(2026, 5, 18)

    # Act
    request = mapper.to_request("Market", registered_at, [branded_product()])

    # Assert
    assert len(request.products) == 1
    product = request.products[0]
    assert product.name == "Product"
    assert abs(product.price - 1.99) < 0.001
    assert product.quantity == "500g"
    assert product.market_name == "Market"
    assert product.brand_name == "Brand"
    assert product.image_url == "https://example.com/product.png"
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


async def test_authenticates_before_save():
    # Arrange
    market_stub = FakeMarketStub()
    auth_stub = FakeAuthStub()
    repository = make_repository(market_stub, auth_stub)

    # Act
    await repository.save("Market", date(2026, 5, 18), [branded_product()])

    # Assert
    assert len(auth_stub.login_requests) == 1
    assert auth_stub.login_requests[0].username == "scraper"
    assert auth_stub.login_requests[0].password == "password"
    _, metadata = market_stub.add_products_calls[0]
    assert authorization(metadata) == "Bearer token-1"


async def test_reuses_valid_token():
    # Arrange
    market_stub = FakeMarketStub()
    auth_stub = FakeAuthStub()
    repository = make_repository(market_stub, auth_stub)

    # Act
    await repository.save("Market", date(2026, 5, 18), [branded_product("Product 1")])
    await repository.save("Market", date(2026, 5, 19), [branded_product("Product 2")])

    # Assert
    assert len(auth_stub.login_requests) == 1
    assert len(market_stub.add_products_calls) == 2


async def test_refreshes_expired_token():
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
    _, first_metadata = market_stub.add_products_calls[0]
    _, second_metadata = market_stub.add_products_calls[1]
    assert authorization(first_metadata) == "Bearer token-1"
    assert authorization(second_metadata) == "Bearer token-2"
