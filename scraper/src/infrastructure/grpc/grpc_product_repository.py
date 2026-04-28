import logging
from dataclasses import dataclass
from datetime import UTC, datetime
from typing import override

import grpc.aio
from google.protobuf.timestamp_pb2 import Timestamp

from application.abstractions import ProductRepository
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
    async def save(self, market_name: str, products: list[Product]) -> None:
        await self.__ensure_authenticated()

        registered_at = self.__now_as_timestamp()
        request = market_service_pb2.AddProductsRequest(  # type: ignore
            products=[
                domain_pb2.Product(  # type: ignore
                    name=p.name,
                    price=p.price,
                    quantity=p.quantity,
                    market_name=market_name,
                    brand_name=p.brand,
                )
                for p in products
                if p.brand is not None
            ],
            registered_at=registered_at,
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
            self.__logger.error(
                "Failed to save products for market '%s': %s",
                market_name,
                e.trailing_metadata(),
                exc_info=e,
            )
            raise

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

    def __now_as_timestamp(self) -> Timestamp:
        timestamp = Timestamp()
        timestamp.FromDatetime(datetime.now(UTC))
        return timestamp


@dataclass
class Token:
    value: str
    expires_at: datetime
