import json
import logging
from datetime import date
from typing import Never

import httpx
import pytest
from opentelemetry.instrumentation.httpx import HTTPXClientInstrumentor
from opentelemetry.sdk.trace import TracerProvider

from application.abstractions import RepositorySaveException
from domain import Product
from infrastructure.rest.rest_product_repository import RestProductRepository


def product(brand: str | None = "Brand") -> Product:
    return Product("raw", "Milk", 1.04, 1, "https://example.com/milk.jpg", brand, "l")


def token_response(
    value: str = "machine-token", expires_at: str = "9999-01-01T00:00:00+00:00"
) -> dict[str, str]:
    return {"accessToken": value, "tokenType": "Bearer", "expiresAt": expires_at}


def token_transport(
    snapshot_handler: httpx.MockTransport | None = None,
) -> httpx.MockTransport:
    def respond(request: httpx.Request) -> httpx.Response:
        if request.url.path.endswith("/auth/tokens"):
            return httpx.Response(200, json=token_response())
        if snapshot_handler is not None:
            return snapshot_handler.handle_request(request)
        return httpx.Response(204)

    return httpx.MockTransport(respond)


async def test_posts_existing_fields_without_raw_content() -> None:
    requests: list[httpx.Request] = []

    def respond(request: httpx.Request) -> httpx.Response:
        requests.append(request)
        return httpx.Response(204)

    async with httpx.AsyncClient(
        base_url="http://server/api/v1",
        transport=token_transport(httpx.MockTransport(respond)),
    ) as client:
        repository = RestProductRepository(client, "scraper", "password")
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


async def test_uses_scraper_credentials_to_get_machine_token() -> None:
    token_requests: list[httpx.Request] = []

    def respond(request: httpx.Request) -> httpx.Response:
        if request.url.path.endswith("/auth/tokens"):
            token_requests.append(request)
            return httpx.Response(200, json=token_response())
        return httpx.Response(204)

    async with httpx.AsyncClient(
        base_url="http://server/api/v1", transport=httpx.MockTransport(respond)
    ) as client:
        repository = RestProductRepository(client, "scraper", "password")
        await repository.save("Market", date(2026, 9, 15), [product()])

    assert (
        str(token_requests[0].url),
        json.loads(token_requests[0].content),
    ) == (
        "http://server/api/v1/auth/tokens",
        {"username": "scraper", "password": "password"},
    )


async def test_propagates_current_trace_to_token_request() -> None:
    traceparent: str | None = None

    def respond(request: httpx.Request) -> httpx.Response:
        nonlocal traceparent
        if request.url.path.endswith("/auth/tokens"):
            traceparent = request.headers.get("traceparent")
            return httpx.Response(200, json=token_response())
        return httpx.Response(204)

    tracer_provider = TracerProvider()
    tracer = tracer_provider.get_tracer("test")
    async with httpx.AsyncClient(
        base_url="http://server/api/v1", transport=httpx.MockTransport(respond)
    ) as client:
        HTTPXClientInstrumentor.instrument_client(
            client, tracer_provider=tracer_provider
        )
        repository = RestProductRepository(client, "scraper", "password")
        try:
            with tracer.start_as_current_span("scrape_all_markets") as root_span:
                await repository.save("Market", date(2026, 9, 15), [product()])
                expected_trace_id = format(
                    root_span.get_span_context().trace_id, "032x"
                )
        finally:
            HTTPXClientInstrumentor.uninstrument_client(client)

    assert traceparent is not None and traceparent.split("-")[1] == expected_trace_id


@pytest.mark.parametrize("status", [200, 400, 401, 403, 413, 500, 503])
async def test_unsuccessful_or_unexpected_response_triggers_fallback(
    status: int,
) -> None:
    def respond(request: httpx.Request) -> httpx.Response:
        return httpx.Response(status)

    async with httpx.AsyncClient(
        base_url="http://server/api/v1",
        transport=token_transport(httpx.MockTransport(respond)),
    ) as client:
        repository = RestProductRepository(client, "scraper", "password")
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
        base_url="http://server/api/v1",
        transport=token_transport(httpx.MockTransport(respond)),
    ) as client:
        repository = RestProductRepository(client, "scraper", "password")
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
        base_url="http://server/api/v1",
        transport=token_transport(httpx.MockTransport(respond)),
    ) as client:
        repository = RestProductRepository(client, "scraper", "password")
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
        base_url="http://server/api/v1",
        transport=token_transport(httpx.MockTransport(timeout)),
    ) as client:
        repository = RestProductRepository(client, "scraper", "password")
        with pytest.raises(RepositorySaveException):
            await repository.save("Market", date(2026, 9, 15), [product()])


@pytest.mark.parametrize("status", [401, 503])
async def test_authentication_http_failure_triggers_fallback(status: int) -> None:
    def respond(request: httpx.Request) -> httpx.Response:
        assert request.url.path.endswith("/auth/tokens")
        return httpx.Response(status)

    async with httpx.AsyncClient(
        base_url="http://server/api/v1", transport=httpx.MockTransport(respond)
    ) as client:
        repository = RestProductRepository(client, "scraper", "password")
        with pytest.raises(RepositorySaveException):
            await repository.save("Market", date(2026, 9, 15), [product()])


@pytest.mark.parametrize(
    "body",
    [
        {
            "accessToken": "jwt",
            "tokenType": "Basic",
            "expiresAt": "9999-01-01T00:00:00+00:00",
        },
        {"accessToken": "jwt", "tokenType": "Bearer"},
        {"accessToken": "jwt", "tokenType": "Bearer", "expiresAt": "invalid"},
    ],
)
async def test_invalid_token_response_triggers_fallback(body: dict[str, str]) -> None:
    def respond(request: httpx.Request) -> httpx.Response:
        assert request.url.path.endswith("/auth/tokens")
        return httpx.Response(200, json=body)

    async with httpx.AsyncClient(
        base_url="http://server/api/v1", transport=httpx.MockTransport(respond)
    ) as client:
        repository = RestProductRepository(client, "scraper", "password")
        with pytest.raises(RepositorySaveException):
            await repository.save("Market", date(2026, 9, 15), [product()])


async def test_authentication_timeout_triggers_fallback() -> None:
    def timeout(request: httpx.Request) -> Never:
        raise httpx.ReadTimeout("Timeout", request=request)

    async with httpx.AsyncClient(
        base_url="http://server/api/v1", transport=httpx.MockTransport(timeout)
    ) as client:
        repository = RestProductRepository(client, "scraper", "password")
        with pytest.raises(RepositorySaveException):
            await repository.save("Market", date(2026, 9, 15), [product()])


@pytest.mark.parametrize("expired", [False, True])
async def test_reuses_valid_token_and_refreshes_expired_token(expired: bool) -> None:
    token_requests = 0
    authorizations: list[str] = []

    def respond(request: httpx.Request) -> httpx.Response:
        nonlocal token_requests
        if request.url.path.endswith("/auth/tokens"):
            token_requests += 1
            expiration = (
                "2000-01-01T00:00:00+00:00" if expired else "9999-01-01T00:00:00+00:00"
            )
            return httpx.Response(
                200,
                json=token_response(
                    "first" if token_requests == 1 else "second", expiration
                ),
            )
        authorizations.append(request.headers["Authorization"])
        return httpx.Response(204)

    async with httpx.AsyncClient(
        base_url="http://server/api/v1", transport=httpx.MockTransport(respond)
    ) as client:
        repository = RestProductRepository(client, "scraper", "password")
        await repository.save("Market", date(2026, 9, 15), [product()])
        await repository.save("Market", date(2026, 9, 16), [product()])

    assert (authorizations, token_requests) == (
        ["Bearer first", "Bearer second" if expired else "Bearer first"],
        2 if expired else 1,
    )


async def test_preserves_existing_filter_for_products_without_brand() -> None:
    requests: list[httpx.Request] = []

    def respond(request: httpx.Request) -> httpx.Response:
        requests.append(request)
        return httpx.Response(204)

    async with httpx.AsyncClient(
        base_url="http://server/api/v1",
        transport=token_transport(httpx.MockTransport(respond)),
    ) as client:
        repository = RestProductRepository(client, "scraper", "password")
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
