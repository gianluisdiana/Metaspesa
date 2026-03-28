import re
from time import sleep
from typing import override

from bs4 import BeautifulSoup, Tag
from selenium.common.exceptions import TimeoutException
from selenium.webdriver.common.by import By
from selenium.webdriver.remote.webdriver import WebDriver
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.support.ui import WebDriverWait

from application.abstractions import MarketWebScrapper
from config import ScraperSettings
from domain import Product


class AlcampoWebScrapper(MarketWebScrapper):
    def __init__(self, driver: WebDriver, settings: ScraperSettings) -> None:
        super().__init__()
        self.__driver = driver
        self.__skipped_categories: set[str] = set(settings.skipped_categories)

    @property
    @override
    def url(self) -> str:
        return "https://www.compraonline.alcampo.es/"

    @override
    def navigate_to_home(self) -> None:
        self.__driver.get(self.url)

    @override
    def close_popups(self) -> None:
        WebDriverWait(self.__driver, 5).until(
            EC.element_to_be_clickable(
                (By.CSS_SELECTOR, "#onetrust-reject-all-handler")
            )
        ).click()

        WebDriverWait(self.__driver, 5).until(
            EC.invisibility_of_element_located(
                (By.CSS_SELECTOR, "#onetrust-reject-all-handler")
            )
        )

        WebDriverWait(self.__driver, 10).until(
            EC.element_to_be_clickable(
                (By.XPATH, '//button[@data-test="popup-banner-close-button"]')
            )
        ).click()

    @override
    def set_location(self, postal_code: str) -> None:
        WebDriverWait(self.__driver, 5).until(
            EC.element_to_be_clickable((By.CSS_SELECTOR, "._button--fill_ftyis_140"))
        ).click()

        WebDriverWait(self.__driver, 5).until(
            EC.element_to_be_clickable(
                (By.CSS_SELECTOR, "._button--primary_ftyis_42 > span")
            )
        ).click()

        WebDriverWait(self.__driver, 5).until(
            EC.presence_of_element_located(
                (By.XPATH, '//input[@data-test="search-input"]')
            )
        ).send_keys(postal_code)

        WebDriverWait(self.__driver, 5).until(
            EC.element_to_be_clickable(
                (By.XPATH, f'//span[contains(text(), "{postal_code}")]')
            )
        ).click()

        WebDriverWait(self.__driver, 5).until(
            EC.element_to_be_clickable(
                (By.XPATH, '//span[text()="Confirmar ubicación"]')
            )
        ).click()

        WebDriverWait(self.__driver, 5).until(
            EC.element_to_be_clickable(
                (By.XPATH, '//button[@data-test="choose-delivery-method-submit"]')
            )
        ).click()

        self.__driver.refresh()

    @override
    def navigate_to_categories(self) -> None:
        WebDriverWait(self.__driver, 5).until(
            EC.element_to_be_clickable((By.CSS_SELECTOR, ".dropdown-item-button"))
        ).click()

        WebDriverWait(self.__driver, 5).until(
            EC.element_to_be_clickable((By.XPATH, "//span[text()='Todo el catálogo']"))
        ).click()

    @override
    def get_categories(self) -> list[str]:
        WebDriverWait(self.__driver, 5).until(
            EC.presence_of_element_located((By.CSS_SELECTOR, ".salt-m-t--0"))
        )

        soup = BeautifulSoup(self.__driver.page_source, "html.parser")
        category_tags = soup.select(".salt-m-t--0 li")
        categories: list[str] = [
            category_tag.text
            for category_tag in category_tags
            if category_tag.text not in self.__skipped_categories
        ]

        return categories

    @override
    def scrape_category(self, category: str) -> list[Product]:
        category_scrapper = AlcampoCategoryScrapper(self.__driver, category)
        return category_scrapper.scrape()


class AlcampoCategoryScrapper:
    def __init__(self, driver: WebDriver, category: str) -> None:
        self.driver = driver
        self.category = category

    def scrape(self) -> list[Product]:
        original_url = self.driver.current_url
        self.__navigate_to_category()

        subcategories = self.__get_subcategories()

        products: list[Product] = []
        for subcategory in subcategories:
            self.__try_close_popups()
            products += self.__get_products(subcategory)

        self.driver.get(original_url)
        return products

    def __navigate_to_category(self) -> None:
        WebDriverWait(self.driver, 5).until(
            EC.element_to_be_clickable((By.XPATH, f'//a[text()="{self.category}"]'))
        ).click()

        WebDriverWait(self.driver, 5).until(
            EC.invisibility_of_element_located(
                (By.XPATH, f'//a[text()="{self.category}"]')
            )
        )

    def __get_subcategories(self) -> list[str]:
        WebDriverWait(self.driver, 5).until(
            EC.presence_of_element_located((By.CSS_SELECTOR, ".salt-m-t--0"))
        )

        soup = BeautifulSoup(self.driver.page_source, "html.parser")
        subcategory_tags = soup.select(".salt-m-t--0 li")
        subcategories: list[str] = [
            subcategory_tag.text for subcategory_tag in subcategory_tags
        ]

        return subcategories

    def __get_products(self, subcategory: str) -> list[Product]:
        original_url = self.driver.current_url
        WebDriverWait(self.driver, 5).until(
            EC.element_to_be_clickable((By.XPATH, f'//a[text()="{subcategory}"]'))
        ).click()

        WebDriverWait(self.driver, 5).until(
            EC.presence_of_element_located((By.XPATH, "//h3[@data-test='fop-title']"))
        )

        products: list[Product] = []
        last_products_count: int = 0
        while True:
            soup = BeautifulSoup(self.driver.page_source, "html.parser")

            product_tags = soup.select("div.product-card-container")
            new_products_count: int = len(product_tags)
            visible_products = self.__get_visible_products(product_tags)
            products += visible_products

            if new_products_count == last_products_count:
                break
            last_products_count = new_products_count
            self.driver.execute_script(  # type: ignore
                "window.scrollTo(0, document.body.scrollHeight - 1200);"
            )

            sleep(1)

        self.driver.get(original_url)

        return self.__keep_unique_products(products)

    def __keep_unique_products(self, products: list[Product]) -> list[Product]:
        unique_products: list[Product] = []
        seen_names: set[str] = set()
        for product in products:
            if product.name not in seen_names:
                unique_products.append(product)
                seen_names.add(product.name)
        return unique_products

    def __get_visible_products(self, product_tags: list[Tag]) -> list[Product]:
        products: list[Product] = []
        for product_tag in product_tags:
            if not product_tag.select('h3[data-test="fop-title"]'):
                continue
            product = self.__parse_product_from_tag(product_tag)
            products.append(product)
        return products

    def __parse_product_from_tag(self, product_tag: Tag) -> Product:
        name = str(product_tag.select('h3[data-test="fop-title"]')[0].text)
        quantity = str(product_tag.select('div[data-test="fop-size"] span')[0].text)
        price = float(
            re.sub(
                r"\s+",
                "",
                str(product_tag.select('span[data-test="fop-price"]')[0].text)
                .replace("€", "")
                .replace(",", "."),
            )
        )
        return Product(name=name, quantity=quantity, price=price)

    def __try_close_popups(self) -> None:
        try:
            WebDriverWait(self.driver, 1).until(
                EC.element_to_be_clickable(
                    (By.XPATH, '//button[@data-test="popup-banner-close-button"]')
                )
            ).click()
        except TimeoutException:
            pass
