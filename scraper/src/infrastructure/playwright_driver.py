from __future__ import annotations

from typing import override

from playwright.async_api import Browser, Page, Playwright, async_playwright

from infrastructure.web_driver import Selector, WebDriver


class PlaywrightDriver(WebDriver):
    def __init__(self, playwright: Playwright, browser: Browser, page: Page) -> None:
        self.__playwright = playwright
        self.__browser = browser
        self.__page = page

    @classmethod
    async def create(cls, headless: bool = True) -> PlaywrightDriver:
        playwright = await async_playwright().start()
        browser = await playwright.firefox.launch(headless=headless)
        page = await browser.new_page()
        return cls(playwright, browser, page)

    @override
    async def get(self, url: str) -> None:
        await self.__page.goto(url)

    @override
    async def refresh(self) -> None:
        await self.__page.reload()

    @property
    @override
    def current_url(self) -> str:
        return self.__page.url

    @override
    async def page_source(self) -> str:
        return await self.__page.content()

    @override
    async def execute_script(self, script: str) -> None:
        await self.__page.evaluate(script)

    @override
    async def wait_and_click(self, selector: Selector, timeout: float = 5) -> None:
        target = self.__get_selector_target(selector)
        await self.__page.wait_for_selector(target, timeout=int(timeout * 1_000))
        await self.__page.locator(target).click()

    @override
    async def wait_for_presence(self, selector: Selector, timeout: float = 5) -> None:
        target = self.__get_selector_target(selector)
        await self.__page.wait_for_selector(target, timeout=int(timeout * 1_000))

    @override
    async def wait_for_invisibility_css(
        self, selector: str, timeout: float = 5
    ) -> None:
        await self.__page.wait_for_selector(
            selector, state="hidden", timeout=int(timeout * 1_000)
        )

    @override
    async def wait_and_send_keys(
        self, selector: Selector, text: str, timeout: float = 5
    ) -> None:
        target = self.__get_selector_target(selector)
        locator = self.__page.locator(target)
        await locator.wait_for(timeout=int(timeout * 1_000))
        await locator.fill(text)

    @override
    async def close(self) -> None:
        try:
            await self.__page.close()
        finally:
            try:
                await self.__browser.close()
            finally:
                await self.__playwright.stop()

    @override
    async def quit(self) -> None:
        await self.close()

    @staticmethod
    def __get_selector_target(selector: Selector) -> str:
        return selector.target if selector.type == "css" else f"xpath={selector.target}"
