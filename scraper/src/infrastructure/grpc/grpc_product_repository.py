from datetime import UTC, datetime
from typing import override

import grpc.aio
from google.protobuf.timestamp_pb2 import Timestamp

from application.abstractions import ProductRepository
from domain import Product as DomainProduct
from infrastructure.grpc.protos import market_service_pb2 as pb2
from infrastructure.grpc.protos import market_service_pb2_grpc as pb2_grpc


class GrpcProductRepository(ProductRepository):
    def __init__(self, channel: grpc.aio.Channel) -> None:
        self.__stub = pb2_grpc.MarketServiceStub(channel)

    @override
    async def save(self, market_name: str, products: list[DomainProduct]) -> None:
        registered_at = __now_as_timestamp()

        await self.__stub.AddProducts(  # type: ignore
            pb2.AddProductsRequest(  # type: ignore
                products=[
                    pb2.Product(  # type: ignore
                        name=p.name,
                        price=p.price,
                        quantity=p.quantity,
                        market_name=market_name,
                        brand_name=p.brand,
                    )
                    for p in products
                    if p.brand is not None and p.brand != "" and p.quantity != ""
                ],
                registered_at=registered_at,
            )
        )


def __now_as_timestamp() -> Timestamp:
    ts = Timestamp()
    ts.FromDatetime(datetime.now(UTC))
    return ts
