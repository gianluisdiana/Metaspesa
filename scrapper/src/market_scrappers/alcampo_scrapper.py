from typing import override

from bs4 import BeautifulSoup
from selenium.webdriver.common.by import By
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.support.ui import WebDriverWait

from domain import Product
from market_scrappers.scrapper import Scrapper


class AlcampoScrapper(Scrapper):
    @override
    def _close_popups(self) -> None:
        WebDriverWait(self.driver, 5).until(
            EC.element_to_be_clickable(
                (By.CSS_SELECTOR, "#onetrust-reject-all-handler")
            )
        ).click()

        WebDriverWait(self.driver, 5).until(
            EC.element_to_be_clickable(
                (By.XPATH, '//button[@data-test="popup-banner-close-button"]')
            )
        ).click()

    @override
    def _set_location(self, postal_code: str) -> None:
        WebDriverWait(self.driver, 5).until(
            EC.element_to_be_clickable((By.CSS_SELECTOR, "._button--fill_ftyis_140"))
        ).click()

        WebDriverWait(self.driver, 5).until(
            EC.element_to_be_clickable(
                (By.CSS_SELECTOR, "._button--primary_ftyis_42 > span")
            )
        ).click()

        WebDriverWait(self.driver, 5).until(
            EC.presence_of_element_located(
                (By.XPATH, '//input[@data-test="search-input"]')
            )
        ).send_keys(postal_code)

        WebDriverWait(self.driver, 5).until(
            EC.element_to_be_clickable(
                (By.XPATH, f'//span[contains(text(), "{postal_code}")]')
            )
        ).click()

        WebDriverWait(self.driver, 5).until(
            EC.element_to_be_clickable(
                (By.XPATH, '//span[text()="Confirmar ubicación"]')
            )
        ).click()

        WebDriverWait(self.driver, 5).until(
            EC.element_to_be_clickable(
                (By.XPATH, '//button[@data-test="choose-delivery-method-submit"]')
            )
        ).click()

        self.driver.refresh()

    @override
    def _navigate_to_categories(self) -> None:
        WebDriverWait(self.driver, 5).until(
            EC.element_to_be_clickable((By.CSS_SELECTOR, ".dropdown-item-button"))
        ).click()

        WebDriverWait(self.driver, 5).until(
            EC.element_to_be_clickable((By.XPATH, "//span[text()='Todo el catálogo']"))
        ).click()

    @override
    def _get_categories(self) -> list[str]:
        WebDriverWait(self.driver, 5).until(
            EC.presence_of_element_located((By.CSS_SELECTOR, ".salt-m-t--0"))
        )

        soup = BeautifulSoup(self.driver.page_source, "html.parser")
        product_category_tags = soup.select(".salt-m-t--0 li")
        categories: list[str] = [
            product_category_tag.text for product_category_tag in product_category_tags
        ]

        return categories

    @override
    def _scrape_category(self, category: str) -> list[Product]:
        return []
