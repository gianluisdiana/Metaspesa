import json
from datetime import UTC, datetime

import httpx
import pytest
from opentelemetry.instrumentation.httpx import HTTPXClientInstrumentor
from opentelemetry.sdk.trace import TracerProvider

from infrastructure.rest.rest_token_client import RestTokenClient, TokenRequestError


def token_response(token_type: str = "Bearer") -> dict[str, str]:
    return {
        "accessToken": "jwt-token",
        "tokenType": token_type,
        "expiresAt": "2026-09-13T12:00:00+00:00",
    }


async def test_propagates_current_trace_to_token_request() -> None:
    # Arrange
    captured_traceparent: str | None = None

    def handler(request: httpx.Request) -> httpx.Response:
        nonlocal captured_traceparent
        captured_traceparent = request.headers.get("traceparent")
        return httpx.Response(200, json=token_response())

    tracer_provider = TracerProvider()
    tracer = tracer_provider.get_tracer("test")
    transport = httpx.MockTransport(handler)
    async with httpx.AsyncClient(
        base_url="http://identity/api/v1", transport=transport
    ) as http_client:
        HTTPXClientInstrumentor.instrument_client(
            http_client, tracer_provider=tracer_provider
        )
        client = RestTokenClient(http_client)

        # Act
        try:
            with tracer.start_as_current_span("scrape_all_markets") as root_span:
                await client.create_token("scraper", "password")
                expected_trace_id: str = format(
                    root_span.get_span_context().trace_id, "032x"
                )
        finally:
            HTTPXClientInstrumentor.uninstrument_client(http_client)

    # Assert
    assert (
        captured_traceparent is not None
        and captured_traceparent.split("-")[1] == expected_trace_id
    )


async def test_posts_machine_credentials_to_token_endpoint() -> None:
    # Arrange
    captured_request: httpx.Request | None = None

    def handler(request: httpx.Request) -> httpx.Response:
        nonlocal captured_request
        captured_request = request
        return httpx.Response(200, json=token_response())

    transport = httpx.MockTransport(handler)
    async with httpx.AsyncClient(
        base_url="http://identity/api/v1", transport=transport
    ) as http_client:
        client = RestTokenClient(http_client)

        # Act
        await client.create_token("scraper", "password")

    # Assert
    assert captured_request is not None
    assert str(captured_request.url) == "http://identity/api/v1/auth/tokens"
    assert json.loads(captured_request.content) == {
        "username": "scraper",
        "password": "password",
    }


async def test_returns_access_token_from_rest_response() -> None:
    # Arrange
    def handler(_request: httpx.Request) -> httpx.Response:
        return httpx.Response(200, json=token_response())

    transport = httpx.MockTransport(handler)
    async with httpx.AsyncClient(
        base_url="http://identity/api/v1", transport=transport
    ) as http_client:
        client = RestTokenClient(http_client)

        # Act
        token = await client.create_token("scraper", "password")

    # Assert
    assert token.value == "jwt-token"


async def test_returns_expiration_from_rest_response() -> None:
    # Arrange
    def handler(_request: httpx.Request) -> httpx.Response:
        return httpx.Response(200, json=token_response())

    transport = httpx.MockTransport(handler)
    async with httpx.AsyncClient(
        base_url="http://identity/api/v1", transport=transport
    ) as http_client:
        client = RestTokenClient(http_client)

        # Act
        token = await client.create_token("scraper", "password")

    # Assert
    assert token.expires_at == datetime(2026, 9, 13, 12, tzinfo=UTC)


async def test_rejects_non_bearer_token_response() -> None:
    # Arrange
    def handler(_request: httpx.Request) -> httpx.Response:
        return httpx.Response(200, json=token_response("Basic"))

    transport = httpx.MockTransport(handler)
    async with httpx.AsyncClient(
        base_url="http://identity/api/v1", transport=transport
    ) as http_client:
        client = RestTokenClient(http_client)

        # Act / Assert
        with pytest.raises(TokenRequestError):
            await client.create_token("scraper", "password")


async def test_rejects_unsuccessful_http_response() -> None:
    # Arrange
    def handler(_request: httpx.Request) -> httpx.Response:
        return httpx.Response(401)

    transport = httpx.MockTransport(handler)
    async with httpx.AsyncClient(
        base_url="http://identity/api/v1", transport=transport
    ) as http_client:
        client = RestTokenClient(http_client)

        # Act / Assert
        with pytest.raises(TokenRequestError):
            await client.create_token("scraper", "password")
