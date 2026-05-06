import pytest
from bs4 import BeautifulSoup

from infrastructure.market_scrapers.alcampo_web_scraper import AlcampoProductTag
from infrastructure.market_scrapers.resilience import MissingProductAttributeError


def product_tag(html: str) -> AlcampoProductTag:
    return AlcampoProductTag(BeautifulSoup(html, "html.parser"))


def test_is_skeleton_by_default():
    # Arrange
    tag = product_tag("<div></div>")

    # Act
    is_skeleton = tag.is_skeleton()

    # Assert
    assert is_skeleton


def test_is_not_skeleton_if_name_is_present():
    # Arrange
    html = """
    <div>
        <h3 data-test="fop-title">Product Name</h3>
    </div>
    """
    tag = product_tag(html)

    # Act
    is_skeleton = tag.is_skeleton()

    # Assert
    assert not is_skeleton


def test_is_not_featured_by_default():
    # Arrange
    tag = product_tag("<div></div>")

    # Act
    is_featured = tag.is_featured()

    # Assert
    assert not is_featured


def test_is_featured_if_contains_featured_flag():
    # Arrange
    html = """
    <div>
        <span data-test="fop-featured"></span>
    </div>
    """
    tag = product_tag(html)

    # Act
    is_featured = tag.is_featured()

    # Assert
    assert is_featured


def test_raises_if_name_is_missing():
    # Arrange
    html = """
    <div>
        <div data-test="fop-size"><span>500g</span></div>
        <span data-test="fop-price">1.99</span>
        <img data-test="lazy-load-image" src="https://example.com/product.png" />
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
        <h3 data-test="fop-title">Product Name</h3>
        <span data-test="fop-price">1.99</span>
        <img data-test="lazy-load-image" src="https://example.com/product.png" />
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
        <h3 data-test="fop-title">Product Name</h3>
        <div data-test="fop-size"><span>500g</span></div>
        <img data-test="lazy-load-image" src="https://example.com/product.png" />
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
        <h3 data-test="fop-title">Product Name</h3>
        <div data-test="fop-size"><span>500g</span></div>
        <span data-test="fop-price">1.99</span>
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
        <h3 data-test="fop-title">  Product Name  </h3>
        <div data-test="fop-size"><span>500g</span></div>
        <span data-test="fop-price">1.99</span>
        <img data-test="lazy-load-image" src="https://example.com/product.png" />
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
        <h3 data-test="fop-title">Product Name</h3>
        <div data-test="fop-size"><span>  500g  </span></div>
        <span data-test="fop-price">1.99</span>
        <img data-test="lazy-load-image" src="https://example.com/product.png" />
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
        <h3 data-test="fop-title">Product Name</h3>
        <div data-test="fop-size"><span>500g</span></div>
        <span data-test="fop-price">1.99</span>
        <img data-test="lazy-load-image" src="https://example.com/product.png" />
    </div>
    """
    tag = product_tag(html)

    # Act
    product = tag.to_product()

    # Assert
    assert abs(product.price - 1.99) < 0.001


def test_returns_image_url_if_required_attributes_are_present():
    # Arrange
    html = """
    <div>
        <h3 data-test="fop-title">Product Name</h3>
        <div data-test="fop-size"><span>500g</span></div>
        <span data-test="fop-price">1.99</span>
        <img data-test="lazy-load-image" src="https://example.com/product.png" />
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
        <h3 data-test="fop-title">Product Name</h3>
        <div data-test="fop-size"><span>500g</span></div>
        <span data-test="fop-price">1.99€</span>
        <img data-test="lazy-load-image" src="https://example.com/product.png" />
    </div>
    """
    tag = product_tag(html)

    # Act
    product = tag.to_product()

    # Assert
    assert abs(product.price - 1.99) < 0.001


def test_replaces_comma_with_dot_in_price():
    # Arrange
    html = """
    <div>
        <h3 data-test="fop-title">Product Name</h3>
        <div data-test="fop-size"><span>500g</span></div>
        <span data-test="fop-price">1,99</span>
        <img data-test="lazy-load-image" src="https://example.com/product.png" />
    </div>
    """
    tag = product_tag(html)

    # Act
    product = tag.to_product()

    # Assert
    assert abs(product.price - 1.99) < 0.001


def test_raises_if_price_is_invalid():
    # Arrange
    html = """
    <div>
        <h3 data-test="fop-title">Product Name</h3>
        <div data-test="fop-size"><span>500g</span></div>
        <span data-test="fop-price">invalid</span>
        <img data-test="lazy-load-image" src="https://example.com/product.png" />
    </div>
    """
    tag = product_tag(html)

    # Act / Assert
    with pytest.raises(MissingProductAttributeError):
        tag.to_product()
