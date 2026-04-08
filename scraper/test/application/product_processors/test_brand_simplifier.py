from application.product_processors import BrandSimplifier
from domain import Product


def test_brand_simplifier_does_not_modify_name_if_no_replacements():
    # Arrange
    simplifier = BrandSimplifier({})
    original_name = "ALCAMPO CULTIVAMOS LO BUENO Compota de manzana"
    product = Product(name=original_name, price=0.5, quantity="1 ud")

    # Act
    result = simplifier.process(product)

    # Assert
    assert result.name == original_name


def test_brand_simplifier_replaces_brand_with_standard_brand():
    # Arrange
    simplifier = BrandSimplifier({"ALCAMPO": ["ALCAMPO CULTIVAMOS LO BUENO"]})
    product = Product(
        name="ALCAMPO CULTIVAMOS LO BUENO Compota de manzana",
        price=0.5,
        quantity="1 ud",
    )

    # Act
    result = simplifier.process(product)

    # Assert
    assert result.name == "ALCAMPO Compota de manzana"


def test_brand_simplifier_replaces_multiple_variants():
    # Arrange
    simplifier = BrandSimplifier({"ALCAMPO": ["ALCAMPO CULTIVAMOS LO BUENO", "AUCHAN"]})
    product = Product(
        name="AUCHAN Compota de manzana",
        price=0.5,
        quantity="1 ud",
    )

    # Act
    result = simplifier.process(product)

    # Assert
    assert result.name == "ALCAMPO Compota de manzana"


def test_brand_simplifier_is_case_insensitive():
    # Arrange
    simplifier = BrandSimplifier({"ALCAMPO": ["ALCAMPO CULTIVAMOS LO BUENO"]})
    product = Product(
        name="alcampo cultivamos lo bueno Compota de manzana",
        price=0.5,
        quantity="1 ud",
    )

    # Act
    result = simplifier.process(product)

    # Assert
    assert result.name == "ALCAMPO Compota de manzana"


def test_brand_simplifier_does_not_modify_if_already_has_brand():
    # Arrange
    simplifier = BrandSimplifier({"ALCAMPO": ["Auchan"]})
    product = Product(
        name="Auchan Compota de manzana", price=0.5, quantity="1 ud", brand="ALCAMPO"
    )

    # Act
    result = simplifier.process(product)

    # Assert
    assert result.name == product.name


def test_brand_simplifier_does_not_add_brand():
    # Arrange
    simplifier = BrandSimplifier({"ALCAMPO": ["Auchan"]})
    product = Product(
        name="Auchan Compota de manzana", price=0.5, quantity="1 ud", brand=None
    )

    # Act
    result = simplifier.process(product)

    # Assert
    assert result.brand is None


def test_brand_simplifier_replaces_repeated_variant():
    # Arrange
    simplifier = BrandSimplifier({"ALCAMPO": ["ALCAMPO CULTIVAMOS"]})
    product = Product(
        name="ALCAMPO CULTIVAMOS Compota de manzana ALCAMPO CULTIVAMOS",
        price=0.5,
        quantity="1 ud",
    )

    # Act
    result = simplifier.process(product)

    # Assert
    assert result.name == "ALCAMPO Compota de manzana ALCAMPO"


def test_brand_simplifier_replaces_all_variants():
    # Arrange
    simplifier = BrandSimplifier({"ALCAMPO": ["ALCAMPO CULTIVAMOS LO BUENO", "AUCHAN"]})
    product = Product(
        name="AUCHAN Compota de manzana ALCAMPO CULTIVAMOS LO BUENO",
        price=0.5,
        quantity="1 ud",
    )

    # Act
    result = simplifier.process(product)

    # Assert
    assert result.name == "ALCAMPO Compota de manzana ALCAMPO"
