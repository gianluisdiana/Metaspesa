import logging

import pytest

from infrastructure.market_scrapers.resilience import (
    MissingProductAttributeError,
    RetryPolicy,
)


async def test_retries_recoverable_failures():
    # Arrange
    logger = logging.getLogger("test_retry_policy")
    call_count = 0

    async def operation() -> str:
        nonlocal call_count
        call_count += 1
        if call_count < 3:
            raise MissingProductAttributeError("name")
        return "success"

    # Act
    result = await RetryPolicy(attempts=3).run(
        operation,
        description="operation",
        logger=logger,
    )

    # Assert
    assert result == "success"
    assert call_count == 3


async def test_calls_recovery_between_attempts():
    # Arrange
    logger = logging.getLogger("test_retry_policy")
    recovery_count = 0

    async def operation() -> str:
        raise MissingProductAttributeError("name")

    async def recover() -> None:
        nonlocal recovery_count
        recovery_count += 1

    # Act
    await RetryPolicy(attempts=3).run(
        operation,
        description="operation",
        logger=logger,
        recover=recover,
    )

    # Assert
    assert recovery_count == 2


async def test_returns_none_after_final_recoverable_failure():
    # Arrange
    logger = logging.getLogger("test_retry_policy")

    async def operation() -> str:
        raise MissingProductAttributeError("name")

    # Act
    result = await RetryPolicy(attempts=3).run(
        operation,
        description="operation",
        logger=logger,
    )

    # Assert
    assert result is None


async def test_does_not_swallow_non_recoverable_exceptions():
    # Arrange
    logger = logging.getLogger("test_retry_policy")

    async def operation() -> str:
        raise ValueError("not recoverable")

    # Act / Assert
    with pytest.raises(ValueError, match="not recoverable"):
        await RetryPolicy(attempts=3).run(
            operation,
            description="operation",
            logger=logger,
        )
