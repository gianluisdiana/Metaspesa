from dataclasses import dataclass


@dataclass
class Product:
    name: str
    price: float
    quantity: str
    brand: str | None = None


@dataclass
class Subcategory:
    name: str
    url: str
