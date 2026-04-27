import os
from pathlib import Path


class SecretNotFoundError(Exception):
    pass


class LocalSecretVault:
    def __init__(self) -> None:
        self.__secrets_dir = Path("/run/secrets")

    def read_secret(self, name: str) -> str:
        secret_path = self.__secrets_dir / name
        if secret_path.exists():
            return secret_path.read_text().strip()

        value = os.getenv(name.upper())
        if value is None:
            raise SecretNotFoundError(
                f"Secret '{name}' not found at {secret_path} "
                f"and env var '{name.upper()}' is not set."
            )
        return value
