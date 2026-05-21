import pytest
from live_markets_helpers import (
    first_productful_subcategory,
    first_subcategories,
    soup_from,
)

from infrastructure.market_scrapers.mercadona_web_scraper import MercadonaWebScraper
from infrastructure.market_scrapers.product_tags import MercadonaProductTag
from infrastructure.web_driver import WebDriver

pytestmark = pytest.mark.integration


async def test_mercadona_sets_location(driver: WebDriver, postal_code: str):
    scraper = MercadonaWebScraper(driver)

    await scraper.set_location(postal_code)

    assert driver.current_url.startswith("https://tienda.mercadona.es")


async def test_mercadona_get_categories_returns_categories(
    driver: WebDriver, postal_code: str
):
    scraper = MercadonaWebScraper(driver)

    await scraper.set_location(postal_code)
    categories = await scraper.get_categories()

    assert categories


async def test_mercadona_get_subcategories_returns_subcategories(
    driver: WebDriver, postal_code: str
):
    scraper = MercadonaWebScraper(driver)

    await scraper.set_location(postal_code)
    categories = await scraper.get_categories()
    subcategories = await first_subcategories(categories, scraper.get_subcategories)

    assert subcategories


async def test_mercadona_scrape_subcategory_returns_products(
    driver: WebDriver, postal_code: str
):
    scraper = MercadonaWebScraper(driver)

    await scraper.set_location(postal_code)
    categories = await scraper.get_categories()
    _, products = await first_productful_subcategory(
        categories, scraper.get_subcategories, scraper.scrape_subcategory
    )

    assert products


async def test_mercadona_live_product_markup_matches_public_parser_contract(
    driver: WebDriver, postal_code: str
):
    scraper = MercadonaWebScraper(driver)

    await scraper.set_location(postal_code)
    categories = await scraper.get_categories()
    await first_productful_subcategory(
        categories, scraper.get_subcategories, scraper.scrape_subcategory
    )
    soup = soup_from(await driver.page_source())
    product_tags = [
        MercadonaProductTag(tag)
        for tag in soup.select("button.product-cell__content-link")
    ]

    assert any(tag.is_ready() and tag.to_product().name for tag in product_tags)
