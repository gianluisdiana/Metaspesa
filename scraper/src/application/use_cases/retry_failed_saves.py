import logging
from datetime import date

from application.abstractions import (
    FallbackProductRepository,
    ProductRepository,
    RepositorySaveException,
)


class RetryFailedSavesCommandHandler:
    def __init__(
        self,
        fallback_repository: FallbackProductRepository,
        main_repository: ProductRepository,
    ) -> None:
        self.__fallback_repository = fallback_repository
        self.__main_repository = main_repository
        self.__logger = logging.getLogger(self.__class__.__name__)

    async def handle(self) -> None:
        markets_and_dates = await self.__fallback_repository.get_markets_and_dates()
        for market_name, register_date in markets_and_dates:
            await self.__retry_saving_in_main_repository(market_name, register_date)

    async def __retry_saving_in_main_repository(
        self, market_name: str, date: date
    ) -> None:
        products = await self.__fallback_repository.get_products_by_market_and_date(
            market_name, date
        )
        formatted_date = date.isoformat()
        try:
            await self.__main_repository.save(market_name, date, products)
            await self.__fallback_repository.remove_old_products(market_name, date)
            self.__logger.info(
                "Successfully retried saving products for market %s registered at %s",
                market_name,
                formatted_date,
                extra={"market_name": market_name, "date": formatted_date},
            )
        except RepositorySaveException as ex:
            self.__logger.exception(
                "Failed to save products for market %s registered at %s",
                market_name,
                formatted_date,
                extra={"market_name": market_name, "date": formatted_date},
                exc_info=ex,
            )
