import re
from typing import override

from bs4 import BeautifulSoup, Tag

from application.abstractions import MarketWebScrapper
from config import ScraperSettings
from domain import Product, Subcategory
from infrastructure.playwright_driver import PlaywrightDriver


class AlcampoWebScrapper(MarketWebScrapper):
    def __init__(self, driver: PlaywrightDriver, settings: ScraperSettings) -> None:
        super().__init__()
        self.__driver = driver
        self.__skipped_categories: set[str] = set(settings.skipped_categories)
        self.__category_urls: dict[str, str] = {}

    @property
    @override
    def url(self) -> str:
        return "https://www.compraonline.alcampo.es"

    @override
    def navigate_to_home(self) -> None:
        self.__driver.get(self.url)

    @override
    def close_popups(self) -> None:
        self.__driver.wait_and_click_css("#onetrust-reject-all-handler")
        try:
            self.__driver.wait_for_invisibility_css("#onetrust-reject-all-handler")
        except Exception:
            pass

        self.__driver.wait_and_click_xpath(
            '//button[@data-test="popup-banner-close-button"]', timeout=10
        )

    @override
    def set_location(self, postal_code: str) -> None:
        self.__driver.wait_and_click_css("._button--fill_ftyis_140")
        self.__driver.wait_and_click_css("._button--primary_ftyis_42 > span")

        self.__driver.wait_for_presence_xpath('//input[@data-test="search-input"]')
        self.__driver.wait_and_send_keys_xpath(
            '//input[@data-test="search-input"]', postal_code
        )

        self.__driver.wait_and_click_xpath(f'//span[contains(text(), "{postal_code}")]')
        self.__driver.wait_and_click_xpath('//span[text()="Confirmar ubicación"]')
        self.__driver.wait_and_click_xpath(
            '//button[@data-test="choose-delivery-method-submit"]'
        )

        self.__driver.refresh()

    @override
    def navigate_to_categories(self) -> None:
        self.__driver.wait_and_click_css(".dropdown-item-button")
        self.__driver.wait_and_click_xpath("//span[text()='Todo el catálogo']")

    @override
    def get_categories(self) -> list[str]:
        self.__driver.wait_for_presence_css(".salt-m-t--0")

        soup = BeautifulSoup(self.__driver.page_source, "html.parser")
        category_tags = soup.select(".salt-m-t--0 li a")
        for tag in category_tags:
            name = tag.text
            if name in self.__skipped_categories:
                continue
            href = str(tag.get("href", "")).removeprefix("/")
            self.__category_urls[name] = f"{self.url}/{href}"

        return list(self.__category_urls.keys())

    @override
    def scrape_category(self, category: str) -> list[Product]:
        category_url = self.__category_urls[category]
        category_scrapper = AlcampoCategoryScrapper(
            self.__driver, category_url, self.url
        )
        return category_scrapper.scrape()


class AlcampoCategoryScrapper:
    def __init__(self, driver: PlaywrightDriver, url: str, base_url: str) -> None:
        self.__driver = driver
        self.__url = url
        self.__base_url = base_url

    def scrape(self) -> list[Product]:
        self.__driver.get(self.__url)

        subcategories = self.__get_subcategories()

        products: list[Product] = []
        for subcategory in subcategories[3:]:
            self.__try_close_popups()
            products += self.__get_products(subcategory)

        return products

    def __get_subcategories(self) -> list[Subcategory]:
        self.__driver.wait_for_presence_css(".salt-m-t--0")

        soup = BeautifulSoup(self.__driver.page_source, "html.parser")
        subcategory_tags = soup.select(".salt-m-t--0 li a")
        return [
            Subcategory(
                name=tag.text,
                url=f"{self.__base_url}/{str(tag.get('href', '')).removeprefix('/')}",
            )
            for tag in subcategory_tags
        ]

    def __get_products(self, subcategory: Subcategory) -> list[Product]:
        self.__driver.get(subcategory.url)

        self.__driver.wait_for_presence_xpath("//h3[@data-test='fop-title']")

        products: list[Product] = []
        while True:
            soup = BeautifulSoup(self.__driver.page_source, "html.parser")

            product_tags = list(
                filter(
                    lambda tag: not tag.select('span[data-test="fop-featured"]'),
                    soup.select("div.product-card-container"),
                )
            )
            visible_products = self.__get_visible_products(product_tags)

            if len(products) >= len(product_tags):
                break

            products += filter(
                lambda p, products=products: p not in products, visible_products
            )
            try:
                self.__driver.execute_script("window.scrollBy(0, window.innerHeight);")
            except Exception:
                break

        return list(products)

    def __get_visible_products(self, product_tags: list[Tag]) -> list[Product]:
        products: list[Product] = []
        for product_tag in product_tags:
            if not product_tag.select('h3[data-test="fop-title"]'):
                continue
            if product_tag.select('span[data-test="fop-featured"]'):
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
            self.__driver.wait_and_click_xpath(
                '//button[@data-test="popup-banner-close-button"]', timeout=1
            )
        except Exception:
            pass
