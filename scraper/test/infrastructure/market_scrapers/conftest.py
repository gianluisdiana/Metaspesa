from pathlib import Path

import pytest

from config import load_config
from infrastructure.playwright_driver import PlaywrightDriver

REPOSITORY_ROOT = Path(__file__).parents[4]


def pytest_collection_modifyitems(
    config: pytest.Config, items: list[pytest.Item]
) -> None:
    if "integration" in config.option.markexpr:
        return

    skip_live = pytest.mark.skip(
        reason="Run live retailer tests with pytest -m integration."
    )
    for item in items:
        if "integration" in item.keywords:
            item.add_marker(skip_live)


@pytest.fixture
def postal_code() -> str:
    config = load_config(REPOSITORY_ROOT / "scraper/config.yaml")
    return config.postal_code


@pytest.fixture
def scraper_config():
    return load_config(REPOSITORY_ROOT / "scraper/config.yaml")


@pytest.fixture
async def driver():
    driver = await PlaywrightDriver.create(headless=True)
    try:
        yield driver
    finally:
        await driver.close()
