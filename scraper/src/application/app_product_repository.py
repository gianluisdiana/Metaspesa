import logging
from typing import override

from application.abstractions import ProductRepository
from domain import Product


class AppProductRepository(ProductRepository):
    def __init__(
        self, main_repository: ProductRepository, fallback_repository: ProductRepository
    ):
        self.__main_repository = main_repository
        self.__fallback_repository = fallback_repository
        self.__logger = logging.getLogger(__name__)

    @override
    async def save(self, market_name: str, products: list[Product]) -> None:
        try:
            await self.__main_repository.save(market_name, products)
        except Exception as e:
            self.__logger.error(
                "Error occurred while saving to main repository", exc_info=e
            )
            await self.__fallback_repository.save(market_name, products)
