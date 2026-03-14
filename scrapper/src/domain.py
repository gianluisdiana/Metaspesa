from dataclasses import dataclass


@dataclass
class Product:
    name: str
    price: float
    quantity: str


@dataclass
class Market:
    name: str
    url: str
