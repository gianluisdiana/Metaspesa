from collections.abc import Awaitable, Callable

from bs4 import BeautifulSoup

from domain import Product, Subcategory


async def first_subcategories(
    categories: list[str],
    get_subcategories: Callable[[str], Awaitable[list[Subcategory]]],
) -> list[Subcategory]:
    for category in categories:
        subcategories = await get_subcategories(category)
        if subcategories:
            return subcategories

    return []


async def first_productful_subcategory(
    categories: list[str],
    get_subcategories: Callable[[str], Awaitable[list[Subcategory]]],
    scrape_subcategory: Callable[[Subcategory], Awaitable[list[Product]]],
) -> tuple[Subcategory | None, list[Product]]:
    for category in categories:
        subcategories = await get_subcategories(category)
        for subcategory in subcategories:
            products = await scrape_subcategory(subcategory)
            if products:
                return subcategory, products

    return None, []


def soup_from(page_source: str) -> BeautifulSoup:
    return BeautifulSoup(page_source, "html.parser")
