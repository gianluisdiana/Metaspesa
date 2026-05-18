import pytest
from pydantic import ValidationError

from config import load_config


def test_loads_postal_code_and_markets(tmp_path):
    # Arrange
    config_file = tmp_path / "config.yaml"
    config_file.write_text(
        """
postal_code: "38320"
markets:
  - "Alcampo"
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
""".strip(),
        encoding="utf-8",
    )

    # Act
    config = load_config(config_file)

    # Assert
    assert config.postal_code == "38320"
    assert config.markets == ["Alcampo"]


def test_raises_if_postal_code_is_missing(tmp_path):
    # Arrange
    config_file = tmp_path / "config.yaml"
    config_file.write_text(
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
""".strip(),
        encoding="utf-8",
    )

    # Act / Assert
    with pytest.raises(ValidationError):
        load_config(config_file)
