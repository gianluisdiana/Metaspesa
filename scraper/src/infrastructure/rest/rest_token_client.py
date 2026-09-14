from dataclasses import dataclass
from datetime import datetime
from typing import Protocol

import httpx


@dataclass(frozen=True)
class Token:
    value: str
    expires_at: datetime


class TokenRequestError(Exception):
    """Raised when the identity REST API cannot issue a machine token."""


class TokenClient(Protocol):
    async def create_token(self, username: str, password: str) -> Token: ...


class RestTokenClient:
    def __init__(
        self,
        http_client: httpx.AsyncClient,
    ) -> None:
        self.__token_path = "/auth/tokens"
        self.__http_client = http_client

    async def create_token(self, username: str, password: str) -> Token:
        try:
            response = await self.__http_client.post(
                self.__token_path,
                json={"username": username, "password": password},
            )
            response.raise_for_status()
            content = response.json()
            if content["tokenType"] != "Bearer":
                raise ValueError("Unsupported token type")
            return Token(
                value=content["accessToken"],
                expires_at=datetime.fromisoformat(content["expiresAt"]),
            )
        except (httpx.HTTPError, KeyError, TypeError, ValueError) as error:
            raise TokenRequestError from error
