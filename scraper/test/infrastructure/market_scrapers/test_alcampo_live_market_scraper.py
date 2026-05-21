import pytest
from live_markets_helpers import (
    first_productful_subcategory,
    first_subcategories,
    soup_from,
)

from config import AppConfig
from infrastructure.market_scrapers.alcampo_web_scraper import AlcampoWebScraper
from infrastructure.market_scrapers.product_tags import AlcampoProductTag
from infrastructure.web_driver import WebDriver

pytestmark = pytest.mark.integration


def create_scraper(driver: WebDriver, config: AppConfig) -> AlcampoWebScraper:
    return AlcampoWebScraper(driver, config.scrapers)


async def test_alcampo_sets_location(
    driver: WebDriver, postal_code: str, scraper_config: AppConfig
):
    scraper = create_scraper(driver, scraper_config)

    await scraper.set_location(postal_code)

    assert driver.current_url.startswith("https://www.compraonline.alcampo.es")


async def test_alcampo_get_categories_returns_categories(
    driver: WebDriver, postal_code: str, scraper_config: AppConfig
):
    scraper = create_scraper(driver, scraper_config)

    await scraper.set_location(postal_code)
    categories = await scraper.get_categories()

    assert categories


async def test_alcampo_get_categories_applies_skipped_categories(
    driver: WebDriver, postal_code: str, scraper_config: AppConfig
):
    scraper = create_scraper(driver, scraper_config)

    await scraper.set_location(postal_code)
    categories = await scraper.get_categories()

    assert not set(categories).intersection(scraper_config.scrapers.skipped_categories)


async def test_alcampo_get_subcategories_returns_subcategories(
    driver: WebDriver, postal_code: str, scraper_config: AppConfig
):
    scraper = create_scraper(driver, scraper_config)

    await scraper.set_location(postal_code)
    categories = await scraper.get_categories()
    subcategories = await first_subcategories(categories, scraper.get_subcategories)

    assert subcategories


async def test_alcampo_scrape_subcategory_returns_products(
    driver: WebDriver, postal_code: str, scraper_config: AppConfig
):
    scraper = create_scraper(driver, scraper_config)

    await scraper.set_location(postal_code)
    categories = await scraper.get_categories()
    _, products = await first_productful_subcategory(
        categories, scraper.get_subcategories, scraper.scrape_subcategory
    )

    assert products


async def test_alcampo_live_product_markup_matches_public_parser_contract(
    driver: WebDriver, postal_code: str, scraper_config: AppConfig
):
    scraper = create_scraper(driver, scraper_config)

    await scraper.set_location(postal_code)
    categories = await scraper.get_categories()
    await first_productful_subcategory(
        categories, scraper.get_subcategories, scraper.scrape_subcategory
    )
    soup = soup_from(await driver.page_source())
    product_tags = [
        AlcampoProductTag(tag) for tag in soup.select("div.product-card-container")
    ]

    assert any(
        not tag.is_skeleton() and not tag.is_featured() and tag.to_product().name
        for tag in product_tags
    )
