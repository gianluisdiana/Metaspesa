from __future__ import annotations

import logging
from datetime import UTC, date, datetime
from typing import Any, override

import grpc.aio
from google.protobuf.timestamp_pb2 import Timestamp

from application.abstractions import ProductRepository, RepositorySaveException
from domain import Product
from infrastructure.grpc.protos import (
    domain_pb2,
    market_service_pb2,
    market_service_pb2_grpc,
)
from infrastructure.rest.rest_token_client import Token, TokenClient, TokenRequestError


class GrpcProductRepository(ProductRepository):
    def __init__(
        self,
        channel: grpc.aio.Channel,
        username: str,
        password: str,
        token_client: TokenClient,
        *,
        market_stub: Any | None = None,
        request_mapper: AddProductsRequestMapper | None = None,
    ) -> None:
        self.__market_stub = market_stub or market_service_pb2_grpc.MarketServiceStub(
            channel
        )
        self.__token_client = token_client
        self.__request_mapper = request_mapper or AddProductsRequestMapper()
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

        if self.__token is None:
            raise RepositorySaveException("Authentication did not return a token.")
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
                extra={
                    "market_name": market_name,
                    "trailing_metadata": e.trailing_metadata(),
                },
            )
            raise RepositorySaveException from e

    def __build_request(  # type: ignore
        self, market_name: str, date: date, products: list[Product]
    ) -> market_service_pb2.AddProductsRequest:  # type: ignore
        return self.__request_mapper.to_request(market_name, date, products)  # type: ignore

    async def __ensure_authenticated(self) -> None:
        try:
            if self.__token is None or self.__token.expires_at <= datetime.now(UTC):
                self.__token = await self.__token_client.create_token(
                    self.__username,
                    self.__password,
                )
        except TokenRequestError as e:
            self.__logger.exception(
                "Authentication failed for user '%s'",
                self.__username,
                exc_info=e,
                extra={
                    "username": self.__username,
                },
            )
            raise RepositorySaveException from e


class AddProductsRequestMapper:
    def to_request(  # type: ignore
        self, market_name: str, date: date, products: list[Product]
    ) -> market_service_pb2.AddProductsRequest:  # type: ignore
        registered_at = Timestamp()
        registered_at.FromDatetime(
            datetime.combine(date, datetime.min.time(), tzinfo=UTC)
        )
        request = market_service_pb2.AddProductsRequest(  # type: ignore
            products=[
                domain_pb2.Product(  # type: ignore
                    # raw_content=product.raw_content,
                    name=p.name,
                    price=f"{p.price}",
                    quantity=p.quantity,
                    unit_of_measure=p.unit_of_measure,
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
