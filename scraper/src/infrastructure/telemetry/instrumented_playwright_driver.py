from typing import override

from opentelemetry import trace

from infrastructure.web_driver import Selector, WebDriver


class InstrumentedPlaywrightDriver(WebDriver):
    def __init__(self, driver: WebDriver) -> None:
        self.__driver = driver
        self.__tracer = trace.get_tracer("playwright")

    @override
    async def get(self, url: str) -> None:
        with self.__tracer.start_as_current_span(
            "playwright.get", attributes={"url": url}
        ):
            await self.__driver.get(url)

    @override
    async def refresh(self) -> None:
        with self.__tracer.start_as_current_span("playwright.refresh"):
            await self.__driver.refresh()

    @property
    def current_url(self) -> str:
        return self.__driver.current_url

    @override
    async def page_source(self) -> str:
        with self.__tracer.start_as_current_span("playwright.page_source"):
            return await self.__driver.page_source()

    @override
    async def execute_script(self, script: str) -> None:
        with self.__tracer.start_as_current_span("playwright.execute_script"):
            await self.__driver.execute_script(script)

    @override
    async def wait(self, seconds: float) -> None:
        with self.__tracer.start_as_current_span(
            "playwright.wait", attributes={"seconds": seconds}
        ):
            await self.__driver.wait(seconds)

    @override
    async def wait_and_click(self, selector: Selector, timeout: float = 5) -> None:
        with self.__tracer.start_as_current_span(
            "playwright.wait_and_click",
            attributes={"selector": selector.target, "selector_type": selector.type},
        ):
            await self.__driver.wait_and_click(selector, timeout)

    @override
    async def wait_for_presence(self, selector: Selector, timeout: float = 5) -> None:
        with self.__tracer.start_as_current_span(
            "playwright.wait_for_presence",
            attributes={"selector": selector.target, "selector_type": selector.type},
        ):
            await self.__driver.wait_for_presence(selector, timeout)

    @override
    async def wait_for_invisibility_css(
        self, selector: str, timeout: float = 5
    ) -> None:
        with self.__tracer.start_as_current_span(
            "playwright.wait_for_invisibility_css", attributes={"selector": selector}
        ):
            await self.__driver.wait_for_invisibility_css(selector, timeout)

    @override
    async def wait_and_send_keys(
        self, selector: Selector, text: str, timeout: float = 5
    ) -> None:
        with self.__tracer.start_as_current_span(
            "playwright.wait_and_send_keys",
            attributes={"selector": selector.target, "selector_type": selector.type},
        ):
            await self.__driver.wait_and_send_keys(selector, text, timeout)

    @override
    async def close(self) -> None:
        with self.__tracer.start_as_current_span("playwright.close"):
            await self.__driver.close()

    @override
    async def quit(self) -> None:
        with self.__tracer.start_as_current_span("playwright.quit"):
            await self.__driver.quit()
