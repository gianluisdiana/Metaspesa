import pytest
from bs4 import BeautifulSoup

from infrastructure.market_scrapers.mercadona_web_scraper import MercadonaProductTag
from infrastructure.market_scrapers.resilience import MissingProductAttributeError


def product_tag(html: str) -> MercadonaProductTag:
    return MercadonaProductTag(BeautifulSoup(html, "html.parser"))


def test_raises_if_name_is_missing():
    # Arrange
    html = """
    <div>
        <div class="product-format__size--cell" aria-label="500g"></div>
        <p class="product-price__unit-price">1.99</p>
        <div class="product-cell__image-wrapper">
            <img src="https://example.com/product.png" />
        </div>
    </div>
    """
    tag = product_tag(html)

    # Act / Assert
    with pytest.raises(MissingProductAttributeError):
        tag.to_product()


def test_raises_if_quantity_is_missing():
    # Arrange
    html = """
    <div>
        <h4 class="product-cell__description-name">Product Name</h4>
        <p class="product-price__unit-price">1.99</p>
        <div class="product-cell__image-wrapper">
            <img src="https://example.com/product.png" />
        </div>
    </div>
    """
    tag = product_tag(html)

    # Act / Assert
    with pytest.raises(MissingProductAttributeError):
        tag.to_product()


def test_raises_if_price_is_missing():
    # Arrange
    html = """
    <div>
        <h4 class="product-cell__description-name">Product Name</h4>
        <div class="product-format__size--cell" aria-label="500g"></div>
        <div class="product-cell__image-wrapper">
            <img src="https://example.com/product.png" />
        </div>
    </div>
    """
    tag = product_tag(html)

    # Act / Assert
    with pytest.raises(MissingProductAttributeError):
        tag.to_product()


def test_raises_if_image_url_is_missing():
    # Arrange
    html = """
    <div>
        <h4 class="product-cell__description-name">Product Name</h4>
        <div class="product-format__size--cell" aria-label="500g"></div>
        <p class="product-price__unit-price">1.99</p>
    </div>
    """
    tag = product_tag(html)

    # Act / Assert
    with pytest.raises(MissingProductAttributeError):
        tag.to_product()


def test_returns_name_if_required_attributes_are_present():
    # Arrange
    html = """
    <div>
        <h4 class="product-cell__description-name">  Product Name  </h4>
        <div class="product-format__size--cell" aria-label="500g"></div>
        <p class="product-price__unit-price">1.99</p>
        <div class="product-cell__image-wrapper">
            <img src="https://example.com/product.png" />
        </div>
    </div>
    """
    tag = product_tag(html)

    # Act
    product = tag.to_product()

    # Assert
    assert product.name == "Product Name"


def test_returns_quantity_if_required_attributes_are_present():
    # Arrange
    html = """
    <div>
        <h4 class="product-cell__description-name">Product Name</h4>
        <div class="product-format__size--cell" aria-label="  500g  "></div>
        <p class="product-price__unit-price">1.99</p>
        <div class="product-cell__image-wrapper">
            <img src="https://example.com/product.png" />
        </div>
    </div>
    """
    tag = product_tag(html)

    # Act
    product = tag.to_product()

    # Assert
    assert product.quantity == "500g"


def test_returns_price_if_required_attributes_are_present():
    # Arrange
    html = """
    <div>
        <h4 class="product-cell__description-name">Product Name</h4>
        <div class="product-format__size--cell" aria-label="500g"></div>
        <p class="product-price__unit-price">1.99</p>
        <div class="product-cell__image-wrapper">
            <img src="https://example.com/product.png" />
        </div>
    </div>
    """
    tag = product_tag(html)

    # Act
    product = tag.to_product()

    # Assert
    assert abs(product.price - 1.99) < 0.01


def test_returns_image_url_if_required_attributes_are_present():
    # Arrange
    html = """
    <div>
        <h4 class="product-cell__description-name">Product Name</h4>
        <div class="product-format__size--cell" aria-label="500g"></div>
        <p class="product-price__unit-price">1.99</p>
        <div class="product-cell__image-wrapper">
            <img src="https://example.com/product.png" />
        </div>
    </div>
    """
    tag = product_tag(html)

    # Act
    product = tag.to_product()

    # Assert
    assert product.image_url == "https://example.com/product.png"


def test_removes_euro_symbol_from_price():
    # Arrange
    html = """
    <div>
        <h4 class="product-cell__description-name">Product Name</h4>
        <div class="product-format__size--cell" aria-label="500g"></div>
        <p class="product-price__unit-price">1.99€</p>
        <div class="product-cell__image-wrapper">
            <img src="https://example.com/product.png" />
        </div>
    </div>
    """
    tag = product_tag(html)

    # Act
    product = tag.to_product()

    # Assert
    assert abs(product.price - 1.99) < 0.01


def test_replaces_comma_with_dot_in_price():
    # Arrange
    html = """
    <div>
        <h4 class="product-cell__description-name">Product Name</h4>
        <div class="product-format__size--cell" aria-label="500g"></div>
        <p class="product-price__unit-price">1,99</p>
        <div class="product-cell__image-wrapper">
            <img src="https://example.com/product.png" />
        </div>
    </div>
    """
    tag = product_tag(html)

    # Act
    product = tag.to_product()

    # Assert
    assert abs(product.price - 1.99) < 0.01


def test_raises_if_price_is_invalid():
    # Arrange
    html = """
    <div>
        <h4 class="product-cell__description-name">Product Name</h4>
        <div class="product-format__size--cell" aria-label="500g"></div>
        <p class="product-price__unit-price">invalid</p>
        <div class="product-cell__image-wrapper">
            <img src="https://example.com/product.png" />
        </div>
    </div>
    """
    tag = product_tag(html)

    # Act / Assert
    with pytest.raises(MissingProductAttributeError):
        tag.to_product()
