import pytest

from application.product_processors import QuantityRedundancyRemover
from domain import Product


def test_processor_does_not_remove_quantity_if_does_not_have_it_on_name():
    # Arrange
    remover = QuantityRedundancyRemover()
    product = Product(name="Arándanos", price=1.2, quantity="1 ud")

    # Act
    result = remover.process(product)

    # Assert
    assert result.name == "Arándanos"


def test_processor_removes_quantity_from_name():
    # Arrange
    remover = QuantityRedundancyRemover()
    product = Product(name="Arándanos 125 g", price=1.2, quantity="125g")

    # Act
    result = remover.process(product)

    # Assert
    assert result.name == "Arándanos"


@pytest.mark.parametrize(
    "original,expected_name",
    [
        (
            "ALCAMPO Compota de manzana y fresa pack 4 uds. x100 g.",
            "ALCAMPO Compota de manzana y fresa",
        ),
        ("Arándanos, tarrina de 125 g-", "Arándanos"),
        ("Fresón tarrina de 500 g.", "Fresón"),
        (
            "Auchan ECOLÓGICO Manzanas golden ecológica  Bandeja 700 g.",
            "Auchan ECOLÓGICO Manzanas golden ecológica",
        ),
        ("Manzanas rojas 1 kg.", "Manzanas rojas"),
        (
            "Auchan Compota de manzana y fresa pack 4 uds. x100 g.",
            "Auchan Compota de manzana y fresa",
        ),
        (
            "Auchan Compota de manzana y pera  pack 4 uds. x 100 g.",
            "Auchan Compota de manzana y pera",
        ),
        ("Coco 1 ud.", "Coco"),
        ("Peras ecológicas bandeja de 700 g.", "Peras ecológicas"),
        ("Manzanas reinetas 1 kg.", "Manzanas reinetas"),
        ("Naranjas de zumo malla 5 kg.", "Naranjas de zumo"),
        ("Kiwis ecológicos bandeja de 600 g.", "Kiwis ecológicos"),
        (
            "Auchan Compota de manzana y piña pack 4 uds. x 100 g.",
            "Auchan Compota de manzana y piña",
        ),
        ("Limas, bandeja de 500 gramos", "Limas"),
        ("Plátano bandeja", "Plátano"),
        ("Manzana roja sabor-sabor bandeja 4 uds.", "Manzana roja sabor-sabor"),
        ("Papaya bolsa 1 kilogramo aproximadamente", "Papaya"),
        (
            "GOLDEN Uvas en almíbar peladas y sin semillas pack de 3 uds.x 120 g.",
            "GOLDEN Uvas en almíbar peladas y sin semillas",
        ),
        ("Manzanas Verde Doncella, bandeja 4 uds.", "Manzanas Verde Doncella"),
        ("Naranja malla de 2kg.", "Naranja"),
        (
            "AZABACHE Vino tinto joven botella de 75 cl.",
            "AZABACHE Vino tinto joven",
        ),
        (
            "ECOCESTA Bebida de soja, con alto contenido en proteinas 1 l.",
            "ECOCESTA Bebida de soja, con alto contenido en proteinas",
        ),
        (
            "MUN té verde (kombucha) con sabor a frutos rojos 250 ml.",
            "MUN té verde (kombucha) con sabor a frutos rojos",
        ),
        (
            "VIVER té (kombucha) sabor a frutos rojos 700 ml.",
            "VIVER té (kombucha) sabor a frutos rojos",
        ),
        (
            "CODORNIU Cava seco ecológico benjamin 3 x 20 cl.",
            "CODORNIU Cava seco ecológico benjamin",
        ),
        ("EcoBATEA Zumo mandarina ecológico 1 l.", "EcoBATEA Zumo mandarina ecológico"),
        (
            "KENCKO Smoothie blues para batido de frutas y espirulina azul 2 x 22 g.",
            "KENCKO Smoothie blues para batido de frutas y espirulina azul",
        ),
        (
            "NEUTREX Quitamanchas líquido blanco puro 1,6 L.",
            "NEUTREX Quitamanchas líquido blanco puro",
        ),
        (
            "PRODUCTO ALCAMPO Detergente en polvo fresco y limpio 40 lav 2 Kg.",
            "PRODUCTO ALCAMPO Detergente en polvo fresco y limpio",
        ),
        (
            "FLOR Suavizante concentrado azul FLOR 172 lavados + 25%, 3,87 L.",
            "FLOR Suavizante concentrado azul FLOR + 25%",
        ),
        (
            "WIPP EXPRESS Detergente en cápsulas, con fragancia floral, 55 lavados",
            "WIPP EXPRESS Detergente en cápsulas, con fragancia floral",
        ),
        (
            "PRODUCTO ALCAMPO Detergente de cápsulas 5 en 1 fresh 20 ds",
            "PRODUCTO ALCAMPO Detergente de cápsulas 5 en 1 fresh",
        ),
        (
            "EKOSAN Suavizante colonia EKOSAN 120 ds. 3 l.",
            "EKOSAN Suavizante colonia EKOSAN",
        ),
        ("Arándanos 125 g", "Arándanos"),
    ],
)
def test_processor_removes_quantity_with_extras_from_name(
    original: str, expected_name: str
):
    # Arrange
    remover = QuantityRedundancyRemover()
    product = Product(name=original, price=1.2, quantity="1g")

    # Act
    result = remover.process(product)

    # Assert
    assert result.name == expected_name
