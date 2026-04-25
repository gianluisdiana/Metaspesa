from opentelemetry import trace

from infrastructure.web_driver import WebDriver


class InstrumentedPlaywrightDriver(WebDriver):
    def __init__(self, driver: WebDriver) -> None:
        self.__driver = driver
        self.__tracer = trace.get_tracer("playwright")

    async def get(self, url: str) -> None:
        with self.__tracer.start_as_current_span(
            "playwright.get", attributes={"url": url}
        ):
            await self.__driver.get(url)

    async def refresh(self) -> None:
        with self.__tracer.start_as_current_span("playwright.refresh"):
            await self.__driver.refresh()

    @property
    def current_url(self) -> str:
        return self.__driver.current_url

    async def page_source(self) -> str:
        with self.__tracer.start_as_current_span("playwright.page_source"):
            return await self.__driver.page_source()

    async def execute_script(self, script: str):
        with self.__tracer.start_as_current_span("playwright.execute_script"):
            return await self.__driver.execute_script(script)

    async def wait_and_click_css(self, selector: str, timeout: float = 5) -> None:
        with self.__tracer.start_as_current_span(
            "playwright.wait_and_click_css", attributes={"selector": selector}
        ):
            await self.__driver.wait_and_click_css(selector, timeout)

    async def wait_and_click_xpath(self, xpath: str, timeout: float = 5) -> None:
        with self.__tracer.start_as_current_span(
            "playwright.wait_and_click_xpath", attributes={"xpath": xpath}
        ):
            await self.__driver.wait_and_click_xpath(xpath, timeout)

    async def wait_for_presence_css(self, selector: str, timeout: float = 5) -> None:
        with self.__tracer.start_as_current_span(
            "playwright.wait_for_presence_css", attributes={"selector": selector}
        ):
            await self.__driver.wait_for_presence_css(selector, timeout)

    async def wait_for_presence_xpath(self, xpath: str, timeout: float = 5) -> None:
        with self.__tracer.start_as_current_span(
            "playwright.wait_for_presence_xpath", attributes={"xpath": xpath}
        ):
            await self.__driver.wait_for_presence_xpath(xpath, timeout)

    async def wait_for_invisibility_css(
        self, selector: str, timeout: float = 5
    ) -> None:
        with self.__tracer.start_as_current_span(
            "playwright.wait_for_invisibility_css", attributes={"selector": selector}
        ):
            await self.__driver.wait_for_invisibility_css(selector, timeout)

    async def wait_and_send_keys_xpath(
        self, xpath: str, text: str, timeout: float = 5
    ) -> None:
        with self.__tracer.start_as_current_span(
            "playwright.wait_and_send_keys_xpath", attributes={"xpath": xpath}
        ):
            await self.__driver.wait_and_send_keys_xpath(xpath, text, timeout)

    async def wait_and_send_keys_css(
        self, selector: str, text: str, timeout: float = 5
    ) -> None:
        with self.__tracer.start_as_current_span(
            "playwright.wait_and_send_keys_css", attributes={"selector": selector}
        ):
            await self.__driver.wait_and_send_keys_css(selector, text, timeout)

    async def close(self) -> None:
        with self.__tracer.start_as_current_span("playwright.close"):
            await self.__driver.close()

    async def quit(self) -> None:
        with self.__tracer.start_as_current_span("playwright.quit"):
            await self.__driver.quit()
