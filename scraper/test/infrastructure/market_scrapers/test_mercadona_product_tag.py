from bs4 import BeautifulSoup

from infrastructure.market_scrapers.mercadona_web_scraper import MercadonaProductTag


def test_does_not_return_name_if_not_present():
    # Arrange
    html = """
    <div></div>
    """

    product_tag = MercadonaProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert product.name == ""


def test_returns_name_if_present():
    # Arrange
    expected_name = "Product Name"
    html = f"""
    <div>
        <h4 class="product-cell__description-name">{expected_name}</h4>
    </div>
    """

    product_tag = MercadonaProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert product.name == expected_name


def test_strips_whitespace_from_name():
    # Arrange
    expected_name = "Product Name"
    html = f"""
    <div>
        <h4 class="product-cell__description-name">  {expected_name}  </h4>
    </div>
    """

    product_tag = MercadonaProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert product.name == expected_name


def test_does_not_return_price_if_not_present():
    # Arrange
    html = """
    <div></div>
    """

    product_tag = MercadonaProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert (product.price - 0.0) < 0.01


def test_returns_price_if_present():
    # Arrange
    expected_price = 1.99
    html = f"""
    <div>
        <p class="product-price__unit-price">{expected_price}</p>
    </div>
    """

    product_tag = MercadonaProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert (product.price - expected_price) < 0.01


def test_removes_euro_symbol_from_price():
    # Arrange
    expected_price = 1.99
    html = f"""
    <div>
        <p class="product-price__unit-price">{expected_price}€</p>
    </div>
    """

    product_tag = MercadonaProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert (product.price - expected_price) < 0.01


def test_replaces_comma_with_dot_in_price():
    # Arrange
    expected_price = 1.99
    html = """
    <div>
        <p class="product-price__unit-price">1,99</p>
    </div>
    """

    product_tag = MercadonaProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert (product.price - expected_price) < 0.01


def test_strips_whitespace_from_price():
    # Arrange
    expected_price = 1.99
    html = """
    <div>
        <p class="product-price__unit-price">  1.99  </p>
    </div>
    """

    product_tag = MercadonaProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert (product.price - expected_price) < 0.01


def test_does_not_return_quantity_if_not_present():
    # Arrange
    html = """
    <div></div>
    """

    product_tag = MercadonaProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert product.quantity == ""


def test_returns_quantity_if_present():
    # Arrange
    expected_quantity = "500g"
    html = f"""
    <div>
        <div class="product-format__size--cell" aria-label="{expected_quantity}"></div>
    </div>
    """

    product_tag = MercadonaProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert product.quantity == expected_quantity


def test_removes_whitespace_from_quantity():
    # Arrange
    expected_quantity = "500g"
    html = f"""
    <div>
        <div
            class="product-format__size--cell"
            aria-label="  {expected_quantity}  ">
        </div>
    </div>
    """

    product_tag = MercadonaProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert product.quantity == expected_quantity
