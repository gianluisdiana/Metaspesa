from datetime import UTC, datetime
from typing import override

import grpc.aio
from google.protobuf.timestamp_pb2 import Timestamp

from application.abstractions import ProductRepository
from domain import Product
from infrastructure.grpc.protos import (
    domain_pb2,
    market_service_pb2,
    market_service_pb2_grpc,
)


class GrpcProductRepository(ProductRepository):
    def __init__(self, channel: grpc.aio.Channel) -> None:
        self.__stub = market_service_pb2_grpc.MarketServiceStub(channel)

    @override
    async def save(self, market_name: str, products: list[Product]) -> None:
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

        await self.__stub.AddProducts(request)  # type: ignore

    def __now_as_timestamp(self) -> Timestamp:
        timestamp = Timestamp()
        timestamp.FromDatetime(datetime.now(UTC))
        return timestamp
