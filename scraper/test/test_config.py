import pytest
from pydantic import ValidationError

from config import load_config


def write_config(config_file, content: str) -> None:
    config_file.write_text(content.strip(), encoding="utf-8")


def valid_config_yaml() -> str:
    return """
postal_code: "38320"
markets:
  - "Alcampo"
processor:
  known_brands:
    - "Brand"
  replacements:
    Brand:
      - "Brand Variant"
scrapers:
  skipped_categories:
    - "Category"
fallback_persistence:
  folder_path: "data"
credentials:
  username_secret: "scraper_username"
  password_secret: "scraper_password"
"""


def test_loads_valid_config(tmp_path):
    # Arrange
    config_file = tmp_path / "config.yaml"
    write_config(config_file, valid_config_yaml())

    # Act
    config = load_config(config_file)

    # Assert
    assert config.postal_code == "38320"
    assert config.markets == ["Alcampo"]
    assert config.processor.known_brands == ["Brand"]
    assert config.processor.replacements == {"Brand": ["Brand Variant"]}
    assert config.scrapers.skipped_categories == ["Category"]
    assert str(config.fallback_persistence.folder_path) == "data"
    assert config.credentials.username_secret == "scraper_username"
    assert config.credentials.password_secret == "scraper_password"


def test_raises_if_postal_code_is_missing(tmp_path):
    # Arrange
    config_file = tmp_path / "config.yaml"
    write_config(
        config_file,
        """
markets: []
processor:
  known_brands: []
  replacements: {}
scrapers:
  skipped_categories: []
fallback_persistence:
  folder_path: "data"
credentials:
  username_secret: "scraper_username"
  password_secret: "scraper_password"
""",
    )

    # Act / Assert
    with pytest.raises(ValidationError):
        load_config(config_file)


def test_raises_if_config_path_is_invalid(tmp_path):
    # Arrange
    missing_config_file = tmp_path / "missing.yaml"

    # Act / Assert
    with pytest.raises(FileNotFoundError):
        load_config(missing_config_file)


def test_raises_if_config_is_empty(tmp_path):
    # Arrange
    config_file = tmp_path / "config.yaml"
    config_file.write_text("", encoding="utf-8")

    # Act / Assert
    with pytest.raises(ValidationError):
        load_config(config_file)
