from collections.abc import Awaitable, Callable
from dataclasses import dataclass
from logging import Logger
from typing import TypeVar

from playwright.async_api import Error as PlaywrightError
from playwright.async_api import TimeoutError as PlaywrightTimeoutError


class MissingProductAttributeError(ValueError):
    def __init__(self, attribute: str) -> None:
        super().__init__(f"Missing required product attribute: {attribute}")


SCRAPER_RECOVERABLE_ERRORS = (
    PlaywrightError,
    PlaywrightTimeoutError,
    MissingProductAttributeError,
)

T = TypeVar("T")


@dataclass(frozen=True)
class RetryPolicy:
    attempts: int = 3

    async def run(
        self,
        operation: Callable[[], Awaitable[T]],
        *,
        description: str,
        logger: Logger,
        recover: Callable[[], Awaitable[None]] | None = None,
    ) -> T | None:
        for attempt in range(1, self.attempts + 1):
            try:
                return await operation()
            except SCRAPER_RECOVERABLE_ERRORS as ex:
                if attempt == self.attempts:
                    logger.warning(
                        "Skipping %s after %d failed attempts",
                        description,
                        self.attempts,
                        exc_info=ex,
                    )
                    return None

                logger.warning(
                    "Retrying %s after attempt %d/%d failed",
                    description,
                    attempt,
                    self.attempts,
                    exc_info=ex,
                )
                if recover is not None:
                    await self.__recover(
                        recover, description=description, logger=logger
                    )

        return None

    async def __recover(
        self,
        recover: Callable[[], Awaitable[None]],
        *,
        description: str,
        logger: Logger,
    ) -> None:
        try:
            await recover()
        except SCRAPER_RECOVERABLE_ERRORS as ex:
            logger.warning(
                "Recovery failed before retrying %s",
                description,
                exc_info=ex,
            )
