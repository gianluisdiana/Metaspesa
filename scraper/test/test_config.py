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


@pytest.mark.parametrize(
    ("field_path", "expected"),
    [
        ("postal_code", "38320"),
        ("markets", ["Alcampo"]),
        ("processor.known_brands", ["Brand"]),
        ("processor.replacements", {"Brand": ["Brand Variant"]}),
        ("scrapers.skipped_categories", ["Category"]),
        ("fallback_persistence.folder_path", "data"),
        ("credentials.username_secret", "scraper_username"),
        ("credentials.password_secret", "scraper_password"),
    ],
)
def test_loads_valid_config_value(tmp_path, field_path: str, expected):
    # Arrange
    config_file = tmp_path / "config.yaml"
    write_config(config_file, valid_config_yaml())

    # Act
    config = load_config(config_file)
    value = config
    for attribute in field_path.split("."):
        value = getattr(value, attribute)
    if field_path.endswith("folder_path"):
        value = str(value)

    # Assert
    assert value == expected


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
