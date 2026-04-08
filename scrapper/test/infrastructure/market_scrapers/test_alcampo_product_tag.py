from bs4 import BeautifulSoup

from infrastructure.market_scrappers.alcampo_web_scrapper import AlcampoProductTag


def test_is_skeleton_by_default():
    # Arrange
    html = """
    <div></div>
    """

    tag = AlcampoProductTag(BeautifulSoup(html, "html.parser"))

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

    tag = AlcampoProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    is_skeleton = tag.is_skeleton()

    # Assert
    assert not is_skeleton


def test_is_not_featured_by_default():
    # Arrange
    html = """
    <div></div>
    """

    tag = AlcampoProductTag(BeautifulSoup(html, "html.parser"))

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

    tag = AlcampoProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    is_featured = tag.is_featured()

    # Assert
    assert is_featured


def test_does_not_return_name_if_not_present():
    # Arrange
    html = """
    <div></div>
    """

    product_tag = AlcampoProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert product.name == ""


def test_returns_name_if_present():
    # Arrange
    expected_name = "Product Name"
    html = f"""
    <div>
        <h3 data-test="fop-title">{expected_name}</h3>
    </div>
    """

    product_tag = AlcampoProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert product.name == expected_name


def test_removes_whitespaces_from_name():
    # Arrange
    expected_name = "Product Name"
    html = f"""
    <div>
        <h3 data-test="fop-title">   {expected_name}   </h3>
    </div>
    """

    product_tag = AlcampoProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert product.name == expected_name


def test_does_not_return_quantity_if_not_present():
    # Arrange
    html = """
    <div></div>
    """

    product_tag = AlcampoProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert product.quantity == ""


def test_returns_quantity_if_present():
    # Arrange
    expected_quantity = "500g"
    html = f"""
    <div>
        <div data-test="fop-size">
            <span>{expected_quantity}</span>
        </div>
    </div>
    """

    product_tag = AlcampoProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert product.quantity == expected_quantity


def test_removes_whitespaces_from_quantity():
    # Arrange
    expected_quantity = "500g"
    html = f"""
    <div>
        <div data-test="fop-size">
            <span>   {expected_quantity}   </span>
        </div>
    </div>
    """

    product_tag = AlcampoProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert product.quantity == expected_quantity


def test_does_not_return_price_if_not_present():
    # Arrange
    html = """
    <div></div>
    """

    product_tag = AlcampoProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert abs(product.price - 0.0) < 0.001


def test_returns_price_if_present():
    # Arrange
    expected_price = 1.99
    html = f"""
    <div>
        <span data-test="fop-price">{expected_price}€</span>
    </div>
    """

    product_tag = AlcampoProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert abs(product.price - expected_price) < 0.001


def test_removes_whitespaces_from_price():
    # Arrange
    expected_price = 1.99
    html = f"""
    <div>
        <span data-test="fop-price">   {expected_price}   </span>
    </div>
    """

    product_tag = AlcampoProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert abs(product.price - expected_price) < 0.001


def test_removes_euro_symbol_from_price():
    # Arrange
    expected_price = 1.99
    html = f"""
    <div>
        <span data-test="fop-price">{expected_price}€</span>
    </div>
    """

    product_tag = AlcampoProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert abs(product.price - expected_price) < 0.001


def test_replaces_comma_with_dot_in_price():
    # Arrange
    expected_price = 1.99
    html = """
    <div>
        <span data-test="fop-price">1,99</span>
    </div>
    """

    product_tag = AlcampoProductTag(BeautifulSoup(html, "html.parser"))

    # Act
    product = product_tag.to_product()

    # Assert
    assert abs(product.price - expected_price) < 0.001
