from pathlib import Path

import yaml
from pydantic import BaseModel


class ProcessorSettings(BaseModel):
    known_brands: list[str]
    replacements: dict[str, list[str]]


class ScraperSettings(BaseModel):
    skipped_categories: list[str]


class FallbackPersistenceSettings(BaseModel):
    folder_path: Path


class CredentialsSettings(BaseModel):
    username_secret: str
    password_secret: str


class AppConfig(BaseModel):
    processor: ProcessorSettings
    scrapers: ScraperSettings
    fallback_persistence: FallbackPersistenceSettings
    credentials: CredentialsSettings


def load_config(config_path: str | Path = "config.yaml") -> AppConfig:
    with open(config_path, encoding="utf-8") as file:
        raw_data = yaml.safe_load(file)
    return AppConfig(**raw_data)
