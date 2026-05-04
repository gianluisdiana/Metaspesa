import logging
from dataclasses import dataclass
from datetime import UTC, date, datetime
from typing import override

import grpc.aio
from google.protobuf.timestamp_pb2 import Timestamp

from application.abstractions import ProductRepository, RepositorySaveException
from domain import Product
from infrastructure.grpc.protos import (
    auth_service_pb2,
    auth_service_pb2_grpc,
    domain_pb2,
    market_service_pb2,
    market_service_pb2_grpc,
)


class GrpcProductRepository(ProductRepository):
    def __init__(
        self,
        channel: grpc.aio.Channel,
        username: str,
        password: str,
    ) -> None:
        self.__market_stub = market_service_pb2_grpc.MarketServiceStub(channel)
        self.__auth_stub = auth_service_pb2_grpc.AuthServiceStub(channel)
        self.__username = username
        self.__password = password
        self.__token: Token | None = None
        self.__logger = logging.getLogger(self.__class__.__name__)

    @override
    async def save(self, market_name: str, date: date, products: list[Product]) -> None:
        await self.__ensure_authenticated()

        request: market_service_pb2.AddProductsRequest = self.__build_request(  # type: ignore
            market_name, date, products
        )

        assert self.__token
        try:
            await self.__market_stub.AddProducts(  # type: ignore
                request,
                metadata=grpc.aio.Metadata(
                    ("authorization", f"Bearer {self.__token.value}")
                ),
            )
        except grpc.aio.AioRpcError as e:
            self.__logger.exception(
                "Failed to save products for market '%s'",
                market_name,
                exc_info=e,
                extra={"trailing_metadata": e.trailing_metadata()},
            )
            raise RepositorySaveException from e

    def __build_request(  # type: ignore
        self, market_name: str, date: date, products: list[Product]
    ) -> market_service_pb2.AddProductsRequest:  # type: ignore
        registered_at = Timestamp()
        registered_at.FromDatetime(
            datetime.combine(date, datetime.min.time(), tzinfo=UTC)
        )
        request = market_service_pb2.AddProductsRequest(  # type: ignore
            products=[
                domain_pb2.Product(  # type: ignore
                    name=p.name,
                    price=p.price,
                    quantity=p.quantity,
                    market_name=market_name,
                    brand_name=p.brand,
                    image_url=p.image_url,
                )
                for p in products
                if p.brand is not None
            ],
            registered_at=registered_at,
        )

        return request  # type: ignore

    async def __ensure_authenticated(self) -> None:
        if self.__token is None or self.__token.expires_at <= datetime.now(UTC):
            response = await self.__auth_stub.Login(  # type: ignore
                auth_service_pb2.LoginRequest(  # type: ignore
                    username=self.__username,
                    password=self.__password,
                )
            )

            self.__token = Token(
                value=response.token,  # type: ignore
                expires_at=datetime.fromisoformat(response.expiration_in_utc),  # type: ignore
            )


@dataclass
class Token:
    value: str
    expires_at: datetime
