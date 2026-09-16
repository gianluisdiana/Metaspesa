import logging
from datetime import UTC, date, datetime
from http import HTTPStatus
from typing import TypedDict, cast, override
from urllib.parse import quote

import httpx

from application.abstractions import ProductRepository, RepositorySaveException
from domain import Product
from infrastructure.rest.rest_token_client import Token, TokenClient, TokenRequestError


class SnapshotItemPayload(TypedDict):
    name: str
    price: float
    quantity: float | str
    unitOfMeasure: str
    brandName: str
    imageUrl: str


class SnapshotPayload(TypedDict):
    items: list[SnapshotItemPayload]


class RestProductRepository(ProductRepository):
    def __init__(
        self,
        http_client: httpx.AsyncClient,
        username: str,
        password: str,
        token_client: TokenClient,
    ) -> None:
        self.__http_client = http_client
        self.__username = username
        self.__password = password
        self.__token_client = token_client
        self.__token: Token | None = None
        self.__logger = logging.getLogger(self.__class__.__name__)

    @override
    async def save(self, market_name: str, date: date, products: list[Product]) -> None:
        try:
            if self.__token is None or self.__token.expires_at <= datetime.now(UTC):
                self.__token = await self.__token_client.create_token(
                    self.__username, self.__password
                )
            payload: SnapshotPayload = {
                "items": [
                    {
                        "name": product.name,
                        "price": product.price,
                        "quantity": product.quantity,
                        "unitOfMeasure": product.unit_of_measure,
                        "brandName": product.brand,
                        "imageUrl": product.image_url,
                    }
                    for product in products
                    if product.brand is not None
                ]
            }
            response = await self.__http_client.post(
                f"/markets/{quote(market_name, safe='')}/snapshots/{date.isoformat()}",
                headers={"Authorization": f"Bearer {self.__token.value}"},
                json=payload,
                timeout=120,
            )
            response.raise_for_status()
            if response.status_code != HTTPStatus.NO_CONTENT:
                raise RepositorySaveException("Unexpected ingestion response.")
        except httpx.HTTPStatusError as error:
            reason = self.__response_reason(error.response)
            self.__logger.error(
                "Failed to save products for market %s: %s",
                market_name,
                reason,
                extra={
                    "market_name": market_name,
                    "http_status": error.response.status_code,
                },
            )
            raise RepositorySaveException(reason) from error
        except (httpx.HTTPError, TokenRequestError, TypeError, ValueError) as error:
            raise RepositorySaveException("Could not save market snapshot.") from error

    @staticmethod
    def __response_reason(response: httpx.Response) -> str:
        fallback = f"HTTP {response.status_code}"
        try:
            problem: object = response.json()
        except ValueError:
            return fallback
        if not isinstance(problem, dict):
            return fallback

        fields = cast(dict[object, object], problem)
        parts = [fallback]
        for key in ("code", "title", "detail", "traceId"):
            value = fields.get(key)
            if isinstance(value, str) and value:
                parts.append(f"{key}={value}")
        return " | ".join(parts)
