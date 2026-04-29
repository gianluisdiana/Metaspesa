import asyncio
import logging
import os

import grpc.aio
from opentelemetry.instrumentation.grpc import aio_client_interceptors  # type: ignore

from application.use_case import (
    RetryFailedSavesCommandHandler,
    ScrapeMarketsCommandHandler,
)
from config import AppConfig, load_config
from dependency_injection import (
    create_retry_handler,
    create_scrape_handler,
    create_web_driver,
)
from infrastructure.telemetry.otel import setup_telemetry
from infrastructure.telemetry.scraper_telemetry import ScraperTelemetry
from infrastructure.web_driver import WebDriver


async def main() -> None:
    logging.basicConfig(
        level=logging.INFO,
        format="\033[1m%(asctime)s\033[0m - \033[1m%(levelname)s\033[0m: %(name)s\n\t%(message)s",  # noqa: E501
    )

    telemetry = setup_telemetry(os.getenv("OTEL_EXPORTER_OTLP_ENDPOINT"))

    settings: AppConfig = load_config()
    web_driver: WebDriver = await create_web_driver()

    async with grpc.aio.insecure_channel(
        os.getenv("GRPC_SERVER_URL") or "localhost:5000",
        interceptors=aio_client_interceptors(),  # type: ignore
    ) as channel:
        try:
            scrape_handler: ScrapeMarketsCommandHandler = create_scrape_handler(
                settings, web_driver, channel
            )
            retry_handler: RetryFailedSavesCommandHandler = create_retry_handler(
                settings, channel
            )

            tasks = [
                retry_failed_saves(telemetry, retry_handler),
                scrape(telemetry, scrape_handler),
            ]
            await asyncio.gather(*tasks)
        finally:
            await web_driver.quit()


async def retry_failed_saves(
    telemetry: ScraperTelemetry, retry_handler: RetryFailedSavesCommandHandler
):
    with telemetry.measure_run("retry_failed_saves"):
        await retry_handler.handle()


async def scrape(
    telemetry: ScraperTelemetry, scrape_handler: ScrapeMarketsCommandHandler
):
    with telemetry.measure_run("scrape_all_markets"):
        await scrape_handler.handle("38320")


if __name__ == "__main__":
    asyncio.run(main())
