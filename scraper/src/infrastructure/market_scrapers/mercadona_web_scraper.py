from typing import override

from bs4 import BeautifulSoup, Tag

from application.abstractions import MarketWebScraper
from domain import Product, Subcategory
from infrastructure.playwright_driver import PlaywrightDriver


class MercadonaWebScraper(MarketWebScraper):
    def __init__(self, driver: PlaywrightDriver) -> None:
        super().__init__()
        self.__driver = driver

    @property
    @override
    def url(self) -> str:
        return "https://tienda.mercadona.es"

    @override
    async def navigate_to_home(self) -> None:
        await self.__driver.get(self.url)

    @override
    async def set_location(self, postal_code: str) -> None:
        await self.__driver.wait_for_presence_xpath(
            '//input[@aria-label="Código postal"]'
        )

        await self.__driver.wait_and_send_keys_xpath(
            '//input[@data-testid="postal-code-checker-input"]', postal_code
        )

        await self.__driver.wait_and_click_xpath(
            '//button[@data-testid="postal-code-checker-button"]'
        )

    @override
    async def navigate_to_categories(self) -> None:
        await self.__driver.wait_for_presence_xpath(
            '//a[contains(text(), "Categorías")]'
        )
        await self.__driver.wait_and_click_xpath('//a[contains(text(), "Categorías")]')

    @override
    async def close_popups(self) -> None:
        await self.__driver.wait_and_click_css("button.ui-button:nth-child(3)")

    @override
    async def get_categories(self) -> list[str]:
        await self.__driver.wait_for_presence_css("li.category-menu__item")

        soup = BeautifulSoup(await self.__driver.page_source(), "html.parser")
        product_category_tags = soup.find_all("li", class_="category-menu__item")
        categories: list[str] = [
            (product_category_tag.select_one("label.subhead1-r") or Tag()).text
            for product_category_tag in product_category_tags
        ]

        return categories

    @override
    async def get_subcategories(self, category: str) -> list[Subcategory]:
        await self.__driver.wait_and_click_xpath(
            f'//label[contains(@class, "subhead1-r") and text()="{category}"]',
        )

        await self.__driver.wait_for_presence_css(
            "li.category-menu__item div ul li button.category-item__link"
        )

        soup = BeautifulSoup(await self.__driver.page_source(), "html.parser")
        subcategory_tags = soup.select(
            "li.category-menu__item div ul li button.category-item__link"
        )
        return [
            Subcategory(
                name=tag.text,
                url=f"{self.url}/categories/{tag.get('id', '')}",
            )
            for tag in subcategory_tags
        ]

    @override
    async def scrape_subcategory(self, subcategory: Subcategory) -> list[Product]:
        await self.__driver.get(subcategory.url)

        await self.__driver.wait_for_presence_css("h1.category-detail__title")

        soup = BeautifulSoup(await self.__driver.page_source(), "html.parser")
        product_tags = soup.select("button.product-cell__content-link")
        products: list[Product] = [
            tag.to_product() for tag in map(MercadonaProductTag, product_tags)
        ]

        return products


class MercadonaProductTag:
    def __init__(self, tag: Tag) -> None:
        self.__tag = tag

    def to_product(self) -> Product:
        return Product(name=self.__name, quantity=self.__quantity, price=self.__price)

    @property
    def __name(self) -> str:
        name_tag = self.__tag.select_one("h4.product-cell__description-name")
        return name_tag.text.strip() if name_tag else ""

    @property
    def __quantity(self) -> str:
        quantity_tag = self.__tag.select_one("div.product-format__size--cell")
        return str(quantity_tag.get("aria-label", "")).strip() if quantity_tag else ""

    @property
    def __price(self) -> float:
        price_tag = self.__tag.select_one("p.product-price__unit-price")
        if not price_tag:
            return 0.0

        price = str(price_tag.text).replace("€", "").replace(",", ".").strip()
        return float(price) if price else 0.0
