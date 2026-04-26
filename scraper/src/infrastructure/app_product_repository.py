from typing import override

from application.abstractions import ProductRepository
from domain import Product


class AppProductRepository(ProductRepository):
    def __init__(
        self, main_repository: ProductRepository, fallback_repository: ProductRepository
    ):
        self.main_repository = main_repository
        self.fallback_repository = fallback_repository

    @override
    async def save(self, market_name: str, products: list[Product]) -> None:
        try:
            await self.main_repository.save(market_name, products)
        except Exception:
            await self.fallback_repository.save(market_name, products)
