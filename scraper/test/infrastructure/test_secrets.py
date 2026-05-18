import pytest

from infrastructure.secrets import LocalSecretVault, SecretNotFoundError


def test_reads_secret_from_environment(monkeypatch):
    # Arrange
    monkeypatch.setenv("SCRAPER_USERNAME", "user@example.com")
    vault = LocalSecretVault()

    # Act
    secret = vault.read_secret("scraper_username")

    # Assert
    assert secret == "user@example.com"


def test_raises_if_secret_is_missing(monkeypatch):
    # Arrange
    monkeypatch.delenv("METASPESA_MISSING_SECRET", raising=False)
    vault = LocalSecretVault()

    # Act / Assert
    with pytest.raises(SecretNotFoundError) as ex:
        vault.read_secret("metaspesa_missing_secret")

    assert "metaspesa_missing_secret" in str(ex.value)
    assert "METASPESA_MISSING_SECRET" in str(ex.value)
