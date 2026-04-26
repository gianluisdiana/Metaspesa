import asyncio
import logging
import os

import grpc.aio
from opentelemetry.instrumentation.grpc import aio_client_interceptors  # type: ignore

from application.use_case import ScrapeMarketsCommandHandler
from config import AppConfig, load_config
from dependency_injection import create_handler, create_web_driver
from infrastructure.telemetry.otel import setup_telemetry
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
        os.environ["GRPC_SERVER_URL"],
        interceptors=aio_client_interceptors(),  # type: ignore
    ) as channel:
        try:
            handler: ScrapeMarketsCommandHandler = create_handler(
                settings, web_driver, channel
            )

            with telemetry.measure_run():
                await handler.handle("38320")
        finally:
            await web_driver.quit()


if __name__ == "__main__":
    asyncio.run(main())
