import logging
from dataclasses import dataclass
from datetime import UTC, date, datetime
from http import HTTPStatus
from typing import TypedDict, cast, override
from urllib.parse import quote

import httpx

from application.abstractions import ProductRepository, RepositorySaveException
from domain import Product


@dataclass(frozen=True)
class Token:
    value: str
    expires_at: datetime


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
    ) -> None:
        self.__http_client = http_client
        self.__username = username
        self.__password = password
        self.__token_path = "/auth/tokens"
        self.__token: Token | None = None
        self.__logger = logging.getLogger(self.__class__.__name__)

    @override
    async def save(self, market_name: str, date: date, products: list[Product]) -> None:
        try:
            await self.__try_save(market_name, date, products)
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
        except (httpx.HTTPError, KeyError, TypeError, ValueError) as error:
            raise RepositorySaveException("Could not save market snapshot.") from error

    async def __try_save(
        self, market_name: str, date: date, products: list[Product]
    ) -> None:
        if self.__token is None or self.__token.expires_at <= datetime.now(UTC):
            self.__token = await self.__create_token()
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
        snapshot_path = (
            f"/markets/{quote(market_name, safe='')}/snapshots/{date.isoformat()}"
        )
        response = await self.__http_client.post(
            snapshot_path,
            headers={"Authorization": f"Bearer {self.__token.value}"},
            json=payload,
            timeout=120,
        )
        response.raise_for_status()
        if response.status_code != HTTPStatus.NO_CONTENT:
            raise RepositorySaveException("Unexpected ingestion response.")

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

    async def __create_token(self) -> Token:
        response = await self.__http_client.post(
            self.__token_path,
            json={"username": self.__username, "password": self.__password},
        )
        response.raise_for_status()
        content = response.json()
        if content["tokenType"] != "Bearer":
            raise ValueError("Unsupported token type")
        return Token(
            value=content["accessToken"],
            expires_at=datetime.fromisoformat(content["expiresAt"]),
        )
