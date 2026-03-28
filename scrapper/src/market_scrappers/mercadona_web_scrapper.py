from typing import override

from bs4 import BeautifulSoup, Tag
from selenium.webdriver.common.by import By
from selenium.webdriver.remote.webdriver import WebDriver
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.support.ui import WebDriverWait

from domain import Product
from market_scrappers.market_web_scrapper import MarketWebScrapper


class MercadonaWebScrapper(MarketWebScrapper):
    def __init__(self, driver: WebDriver) -> None:
        super().__init__()
        self.__driver = driver

    @property
    @override
    def url(self) -> str:
        return "https://www.mercadona.es/"

    @override
    def navigate_to_home(self) -> None:
        self.__driver.get(self.url)

    @override
    def set_location(self, postal_code: str) -> None:
        WebDriverWait(self.__driver, 5).until(
            EC.element_to_be_clickable(
                (By.CSS_SELECTOR, 'input[aria-label="Código postal"]')
            )
        ).send_keys(postal_code)

        WebDriverWait(self.__driver, 5).until(
            EC.element_to_be_clickable((By.CLASS_NAME, "postal-code-form__button"))
        ).click()

    @override
    def navigate_to_categories(self) -> None:
        WebDriverWait(self.__driver, 5).until(
            EC.presence_of_element_located(
                (By.XPATH, '//a[contains(text(), "Categorías")]')
            )
        )

        WebDriverWait(self.__driver, 5).until(
            EC.element_to_be_clickable(
                (By.XPATH, '//a[contains(text(), "Categorías")]')
            )
        ).click()

    @override
    def close_popups(self) -> None:
        WebDriverWait(self.__driver, 5).until(
            EC.element_to_be_clickable(
                (By.CSS_SELECTOR, "button.ui-button:nth-child(3)")
            )
        ).click()

    @override
    def get_categories(self) -> list[str]:
        WebDriverWait(self.__driver, 5).until(
            EC.presence_of_element_located((By.CSS_SELECTOR, "li.category-menu__item"))
        )

        soup = BeautifulSoup(self.__driver.page_source, "html.parser")
        product_category_tags = soup.find_all("li", class_="category-menu__item")
        categories: list[str] = [
            (product_category_tag.select_one("label.subhead1-r") or Tag()).text
            for product_category_tag in product_category_tags
        ]

        return categories

    @override
    def scrape_category(self, category: str) -> list[Product]:
        category_scrapper = MercadonaCategoryScrapper(self.__driver, category)
        return category_scrapper.scrape()


class MercadonaCategoryScrapper:
    def __init__(self, driver: WebDriver, category: str):
        self.driver = driver
        self.category = category

    def scrape(self) -> list[Product]:
        self.__expand_category()
        subcategories = self.__get_subcategories()

        products: list[Product] = []
        for subcategory in subcategories:
            products += self.__get_products(subcategory)

        return products

    def __get_products(self, subcategory: str) -> list[Product]:
        WebDriverWait(self.driver, 5).until(
            EC.element_to_be_clickable(
                (
                    By.XPATH,
                    f'//button[contains(@class, "category-item__link") and text()="{subcategory}"]',  # noqa: E501
                )
            )
        ).click()

        WebDriverWait(self.driver, 5).until(
            EC.presence_of_element_located(
                (By.CSS_SELECTOR, "h1.category-detail__title")
            )
        )

        products: list[Product] = []
        soup = BeautifulSoup(self.driver.page_source, "html.parser")
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
        WebDriverWait(self.driver, 5).until(
            EC.element_to_be_clickable(
                (
                    By.XPATH,
                    f'//label[contains(@class, "subhead1-r") and text()="{self.category}"]',  # noqa: E501
                )
            )
        ).click()

        WebDriverWait(self.driver, 5).until(
            EC.presence_of_element_located(
                (
                    By.CSS_SELECTOR,
                    "li.category-menu__item div ul li button.category-item__link",
                )
            )
        )

    def __get_subcategories(self) -> list[str]:
        soup = BeautifulSoup(self.driver.page_source, "html.parser")
        subcategory_tags = soup.select(
            "li.category-menu__item div ul li button.category-item__link"
        )
        subcategories: list[str] = []

        for subcategory_tag in subcategory_tags:
            subcategories.append(subcategory_tag.text)

        return subcategories
