from dataclasses import dataclass


@dataclass
class Product:
    name: str
    price: float
    quantity: str
    brand: str | None = None

    def __eq__(self, value: object) -> bool:
        if not isinstance(value, Product):
            return False

        return (
            self.name == value.name
            and abs(self.price - value.price) < 0.01
            and self.quantity == value.quantity
            and self.brand == value.brand
        )

    def __ne__(self, value: object) -> bool:
        return not self.__eq__(value)

    def __hash__(self) -> int:
        return hash((self.name, self.price, self.quantity, self.brand))


@dataclass
class Subcategory:
    name: str
    url: str
