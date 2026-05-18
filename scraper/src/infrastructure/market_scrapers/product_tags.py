import re
from typing import Protocol
from urllib.parse import urlparse

from bs4 import Tag

from domain import Product
from infrastructure.market_scrapers.resilience import MissingProductAttributeError


class ProductTag(Protocol):
    def is_ready(self) -> bool: ...

    def to_product(self) -> Product: ...


class ProductTagCollection:
    def __init__(self, product_tags: list[ProductTag]) -> None:
        self.__product_tags = product_tags

    @property
    def tags(self) -> list[ProductTag]:
        return self.__product_tags

    def new_products_since(self, last_added_product: Product | None) -> list[Product]:
        visible_products = [
            tag.to_product() for tag in self.__product_tags if tag.is_ready()
        ]
        index_of_last_added_product = (
            visible_products.index(last_added_product)
            if last_added_product is not None and last_added_product in visible_products
            else -1
        )
        return visible_products[index_of_last_added_product + 1 :]


class AlcampoProductTag:
    def __init__(self, tag: Tag) -> None:
        self.__tag = tag

    def to_product(self) -> Product:
        return Product(
            name=self.__name,
            quantity=self.__quantity,
            price=self.__price,
            image_url=self.__image_url,
        )

    def is_skeleton(self) -> bool:
        return self.__text('h3[data-test="fop-title"]') == ""

    def is_ready(self) -> bool:
        return not self.is_skeleton()

    def is_featured(self) -> bool:
        return bool(self.__tag.select('span[data-test="fop-featured"]'))

    @property
    def __name(self) -> str:
        return self.__required_text('h3[data-test="fop-title"]', "name")

    @property
    def __quantity(self) -> str:
        return self.__required_text('div[data-test="fop-size"] span', "quantity")

    @property
    def __price(self) -> float:
        price = self.__required_text('span[data-test="fop-price"]', "price")
        price = re.sub(r"\s+|€|â‚¬", "", price.replace(",", "."))
        if price == "":
            raise MissingProductAttributeError("price")

        try:
            return float(price)
        except ValueError as ex:
            raise MissingProductAttributeError("price") from ex

    @property
    def __image_url(self) -> str:
        image_tag = self.__tag.select_one('img[data-test="lazy-load-image"]')
        image_url = str(image_tag.get("src", "")).strip() if image_tag else ""
        if image_url == "":
            raise MissingProductAttributeError("image_url")
        return image_url

    def __required_text(self, selector: str, attribute: str) -> str:
        text = self.__text(selector)
        if text == "":
            raise MissingProductAttributeError(attribute)
        return text

    def __text(self, selector: str) -> str:
        tag = self.__tag.select_one(selector)
        return str(tag.text).strip() if tag else ""


class MercadonaProductTag:
    def __init__(self, tag: Tag) -> None:
        self.__tag = tag

    def to_product(self) -> Product:
        return Product(
            name=self.__name,
            quantity=self.__quantity,
            price=self.__price,
            image_url=self.__image_url,
        )

    def is_ready(self) -> bool:
        return (
            self.__text("h4.product-cell__description-name") != ""
            and self.__quantity_text != ""
            and self.__text("p.product-price__unit-price") != ""
            and self.__has_valid_image_url()
        )

    @property
    def __name(self) -> str:
        return self.__required_text("h4.product-cell__description-name", "name")

    @property
    def __quantity(self) -> str:
        quantity = self.__quantity_text
        if quantity == "":
            raise MissingProductAttributeError("quantity")
        return quantity

    @property
    def __quantity_text(self) -> str:
        quantity_tag = self.__tag.select_one("div.product-format__size--cell")
        if quantity_tag:
            texts = [text for text in quantity_tag.stripped_strings if text]
            if len(texts) > 1:
                return texts[1].strip()
            if len(texts) == 1:
                return texts[0].strip()
        return ""

    @property
    def __price(self) -> float:
        price = self.__required_text("p.product-price__unit-price", "price")
        price = price.replace("€", "").replace("â‚¬", "").replace(",", ".").strip()
        if price == "":
            raise MissingProductAttributeError("price")

        try:
            return float(price)
        except ValueError as ex:
            raise MissingProductAttributeError("price") from ex

    @property
    def __image_url(self) -> str:
        image_tag = self.__tag.select_one("div.product-cell__image-wrapper img")
        image_url = str(image_tag.get("src", "")).strip() if image_tag else ""
        if not self.__is_valid_url(image_url):
            raise MissingProductAttributeError("image_url")
        return image_url

    def __required_text(self, selector: str, attribute: str) -> str:
        text = self.__text(selector)
        if text == "":
            raise MissingProductAttributeError(attribute)
        return text

    def __text(self, selector: str) -> str:
        tag = self.__tag.select_one(selector)
        return str(tag.text).strip() if tag else ""

    def __has_valid_image_url(self) -> bool:
        image_tag = self.__tag.select_one("div.product-cell__image-wrapper img")
        image_url = str(image_tag.get("src", "")).strip() if image_tag else ""
        return self.__is_valid_url(image_url)

    @staticmethod
    def __is_valid_url(image_url: str) -> bool:
        parsed_url = urlparse(image_url)
        return parsed_url.scheme in {"http", "https"} and parsed_url.netloc != ""
