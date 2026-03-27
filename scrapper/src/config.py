from pathlib import Path

import yaml
from pydantic import BaseModel


class ParsingSettings(BaseModel):
    known_brands: list[str]


class ScraperSettings(BaseModel):
    skipped_categories: list[str]


class AllScrapersSettings(BaseModel):
    alcampo: ScraperSettings


class AppConfig(BaseModel):
    parsing: ParsingSettings
    scrapers: AllScrapersSettings


def load_config(config_path: str | Path = "config.yaml") -> AppConfig:
    with open(config_path, encoding="utf-8") as file:
        raw_data = yaml.safe_load(file)
    return AppConfig(**raw_data)
