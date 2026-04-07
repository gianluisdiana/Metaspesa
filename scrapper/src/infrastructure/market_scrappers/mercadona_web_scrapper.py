from typing import override

from bs4 import BeautifulSoup, Tag

from application.abstractions import MarketWebScrapper
from domain import Product, Subcategory
from infrastructure.playwright_driver import PlaywrightDriver


class MercadonaWebScrapper(MarketWebScrapper):
    def __init__(self, driver: PlaywrightDriver) -> None:
        super().__init__()
        self.__driver = driver

    @property
    @override
    def url(self) -> str:
        return "https://tienda.mercadona.es"

    @override
    def navigate_to_home(self) -> None:
        self.__driver.get(self.url)

    @override
    def set_location(self, postal_code: str) -> None:
        self.__driver.wait_for_presence_xpath('//input[@aria-label="Código postal"]')

        self.__driver.wait_and_send_keys_xpath(
            '//input[@data-testid="postal-code-checker-input"]', postal_code
        )

        self.__driver.wait_and_click_xpath(
            '//button[@data-testid="postal-code-checker-button"]'
        )

    @override
    def navigate_to_categories(self) -> None:
        self.__driver.wait_for_presence_xpath('//a[contains(text(), "Categorías")]')
        self.__driver.wait_and_click_xpath('//a[contains(text(), "Categorías")]')

    @override
    def close_popups(self) -> None:
        self.__driver.wait_and_click_css("button.ui-button:nth-child(3)")

    @override
    def get_categories(self) -> list[str]:
        self.__driver.wait_for_presence_css("li.category-menu__item")

        soup = BeautifulSoup(self.__driver.page_source, "html.parser")
        product_category_tags = soup.find_all("li", class_="category-menu__item")
        categories: list[str] = [
            (product_category_tag.select_one("label.subhead1-r") or Tag()).text
            for product_category_tag in product_category_tags
        ]

        return categories

    @override
    def scrape_category(self, category: str) -> list[Product]:
        category_scrapper = MercadonaCategoryScrapper(self.__driver, category, self.url)
        return category_scrapper.scrape()


class MercadonaCategoryScrapper:
    def __init__(self, driver: PlaywrightDriver, category: str, base_url: str):
        self.__driver = driver
        self.__category = category
        self.__base_url = base_url

    def scrape(self) -> list[Product]:
        self.__expand_category()
        subcategories = self.__get_subcategories()

        products: list[Product] = []
        for subcategory in subcategories:
            products += self.__get_products(subcategory)

        return products

    def __get_products(self, subcategory: Subcategory) -> list[Product]:
        self.__driver.get(subcategory.url)

        self.__driver.wait_for_presence_css("h1.category-detail__title")

        products: list[Product] = []
        soup = BeautifulSoup(self.__driver.page_source, "html.parser")
        product_links = soup.select("button.product-cell__content-link")
        for link in product_links:
            products.append(self.__extract_product_info(link))

        return products

    def __extract_product_info(self, link: Tag) -> Product:
        name = (link.select_one("h4.product-cell__description-name") or Tag()).text

        quantity = str(
            (link.select_one("div.product-format__size--cell") or Tag()).get(
                "aria-label", ""
            )
        )

        price = (
            str((link.select_one("p.product-price__unit-price") or Tag()).text)
            .replace("€", "")
            .replace(",", ".")
            .strip()
        )

        return Product(name=name, price=float(price), quantity=quantity)

    def __expand_category(self) -> None:
        self.__driver.wait_and_click_xpath(
            f'//label[contains(@class, "subhead1-r") and text()="{self.__category}"]',
        )

        self.__driver.wait_for_presence_css(
            "li.category-menu__item div ul li button.category-item__link"
        )

    def __get_subcategories(self) -> list[Subcategory]:
        soup = BeautifulSoup(self.__driver.page_source, "html.parser")
        subcategory_tags = soup.select(
            "li.category-menu__item div ul li button.category-item__link"
        )
        return [
            Subcategory(
                name=tag.text,
                url=f"{self.__base_url}/categories/{tag.get('id', '')}",
            )
            for tag in subcategory_tags
        ]
