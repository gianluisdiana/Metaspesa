import json
import logging
from datetime import UTC, date, datetime
from typing import Never

import httpx
import pytest

from application.abstractions import RepositorySaveException
from domain import Product
from infrastructure.rest.rest_product_repository import RestProductRepository
from infrastructure.rest.rest_token_client import Token, TokenRequestError


def product(brand: str | None = "Brand") -> Product:
    return Product("raw", "Milk", 1.04, 1, "https://example.com/milk.jpg", brand, "l")


class FakeTokenClient:
    def __init__(
        self,
        issued_tokens: list[Token] | None = None,
        error: TokenRequestError | None = None,
    ) -> None:
        self.issued_tokens = issued_tokens or [
            Token("machine-token", datetime(9999, 1, 1, tzinfo=UTC))
        ]
        self.error = error

    async def create_token(self, username: str, password: str) -> Token:
        if self.error is not None:
            raise self.error
        return self.issued_tokens.pop(0)


async def test_posts_existing_fields_without_raw_content() -> None:
    requests: list[httpx.Request] = []

    def respond(request: httpx.Request) -> httpx.Response:
        requests.append(request)
        return httpx.Response(204)

    async with httpx.AsyncClient(
        base_url="http://server/api/v1", transport=httpx.MockTransport(respond)
    ) as client:
        repository = RestProductRepository(
            client, "scraper", "password", FakeTokenClient()
        )
        await repository.save("Market Name", date(2026, 9, 15), [product()])

    assert (
        requests[0].method,
        str(requests[0].url),
        requests[0].headers["Authorization"],
        json.loads(requests[0].content),
    ) == (
        "POST",
        "http://server/api/v1/markets/Market%20Name/snapshots/2026-09-15",
        "Bearer machine-token",
        {
            "items": [
                {
                    "name": "Milk",
                    "price": 1.04,
                    "quantity": 1,
                    "unitOfMeasure": "l",
                    "brandName": "Brand",
                    "imageUrl": "https://example.com/milk.jpg",
                }
            ]
        },
    )


@pytest.mark.parametrize("status", [200, 400, 401, 403, 413, 500, 503])
async def test_unsuccessful_or_unexpected_response_triggers_fallback(
    status: int,
) -> None:
    def respond(request: httpx.Request) -> httpx.Response:
        return httpx.Response(status)

    async with httpx.AsyncClient(
        base_url="http://server/api/v1",
        transport=httpx.MockTransport(respond),
    ) as client:
        repository = RestProductRepository(
            client, "scraper", "password", FakeTokenClient()
        )
        with pytest.raises(RepositorySaveException):
            await repository.save("Market", date(2026, 9, 15), [product()])


async def test_logs_problem_details_for_bad_request(
    caplog: pytest.LogCaptureFixture,
) -> None:
    def respond(request: httpx.Request) -> httpx.Response:
        return httpx.Response(
            400,
            json={
                "type": "https://metaspesa.app/problems/market-validation",
                "title": "Unit 'bottle' is unsupported.",
                "status": 400,
                "code": "Market.Unit.Unsupported",
                "traceId": "trace-123",
            },
        )

    async with httpx.AsyncClient(
        base_url="http://server/api/v1", transport=httpx.MockTransport(respond)
    ) as client:
        repository = RestProductRepository(
            client, "scraper", "password", FakeTokenClient()
        )
        with (
            caplog.at_level(logging.ERROR, logger="RestProductRepository"),
            pytest.raises(RepositorySaveException) as captured,
        ):
            await repository.save("Market", date(2026, 9, 15), [product()])

    assert (
        "Market.Unit.Unsupported" in str(captured.value)
        and "Unit 'bottle' is unsupported." in caplog.text
        and "trace-123" in caplog.text
    )


async def test_logs_status_when_error_body_is_not_problem_details(
    caplog: pytest.LogCaptureFixture,
) -> None:
    def respond(request: httpx.Request) -> httpx.Response:
        return httpx.Response(503, text="Service unavailable")

    async with httpx.AsyncClient(
        base_url="http://server/api/v1", transport=httpx.MockTransport(respond)
    ) as client:
        repository = RestProductRepository(
            client, "scraper", "password", FakeTokenClient()
        )
        with (
            caplog.at_level(logging.ERROR, logger="RestProductRepository"),
            pytest.raises(RepositorySaveException) as captured,
        ):
            await repository.save("Market", date(2026, 9, 15), [product()])

    assert "HTTP 503" in str(captured.value) and "HTTP 503" in caplog.text


async def test_timeout_triggers_fallback() -> None:
    def timeout(request: httpx.Request) -> Never:
        raise httpx.ReadTimeout("Timeout", request=request)

    async with httpx.AsyncClient(
        base_url="http://server/api/v1", transport=httpx.MockTransport(timeout)
    ) as client:
        repository = RestProductRepository(
            client, "scraper", "password", FakeTokenClient()
        )
        with pytest.raises(RepositorySaveException):
            await repository.save("Market", date(2026, 9, 15), [product()])


async def test_authentication_failure_triggers_fallback() -> None:
    token_client = FakeTokenClient(error=TokenRequestError())
    async with httpx.AsyncClient(base_url="http://server/api/v1") as client:
        repository = RestProductRepository(client, "scraper", "password", token_client)
        with pytest.raises(RepositorySaveException):
            await repository.save("Market", date(2026, 9, 15), [product()])


@pytest.mark.parametrize("expired", [False, True])
async def test_reuses_valid_token_and_refreshes_expired_token(expired: bool) -> None:
    token_client = FakeTokenClient(
        [
            Token("first", datetime(2000 if expired else 9999, 1, 1, tzinfo=UTC)),
            Token("second", datetime(9999, 1, 1, tzinfo=UTC)),
        ]
    )
    authorizations: list[str] = []

    def respond(request: httpx.Request) -> httpx.Response:
        authorizations.append(request.headers["Authorization"])
        return httpx.Response(204)

    async with httpx.AsyncClient(
        base_url="http://server/api/v1", transport=httpx.MockTransport(respond)
    ) as client:
        repository = RestProductRepository(client, "scraper", "password", token_client)
        await repository.save("Market", date(2026, 9, 15), [product()])
        await repository.save("Market", date(2026, 9, 16), [product()])

    assert authorizations == [
        "Bearer first",
        "Bearer second" if expired else "Bearer first",
    ]


async def test_preserves_existing_filter_for_products_without_brand() -> None:
    requests: list[httpx.Request] = []

    def respond(request: httpx.Request) -> httpx.Response:
        requests.append(request)
        return httpx.Response(204)

    async with httpx.AsyncClient(
        base_url="http://server/api/v1", transport=httpx.MockTransport(respond)
    ) as client:
        repository = RestProductRepository(
            client, "scraper", "password", FakeTokenClient()
        )
        await repository.save("Market", date(2026, 9, 15), [product(None), product()])

    assert json.loads(requests[0].content) == {
        "items": [
            {
                "name": "Milk",
                "price": 1.04,
                "quantity": 1,
                "unitOfMeasure": "l",
                "brandName": "Brand",
                "imageUrl": "https://example.com/milk.jpg",
            }
        ]
    }
