from __future__ import annotations

from playwright.async_api import Browser, Page, Playwright, async_playwright


class PlaywrightDriver:
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

    async def get(self, url: str) -> None:
        await self.__page.goto(url)

    async def refresh(self) -> None:
        await self.__page.reload()

    @property
    def current_url(self) -> str:
        return self.__page.url

    async def page_source(self) -> str:
        return await self.__page.content()

    async def execute_script(self, script: str):
        return await self.__page.evaluate(script)

    async def wait_and_click_css(self, selector: str, timeout: float = 5) -> None:
        await self.__page.wait_for_selector(selector, timeout=int(timeout * 1_000))
        await self.__page.locator(selector).click()

    async def wait_and_click_xpath(self, xpath: str, timeout: float = 5) -> None:
        await self.__page.wait_for_selector(
            f"xpath={xpath}", timeout=int(timeout * 1_000)
        )
        await self.__page.locator(f"xpath={xpath}").click()

    async def wait_for_presence_css(self, selector: str, timeout: float = 5) -> None:
        await self.__page.wait_for_selector(selector, timeout=int(timeout * 1_000))

    async def wait_for_presence_xpath(self, xpath: str, timeout: float = 5) -> None:
        await self.__page.wait_for_selector(
            f"xpath={xpath}", timeout=int(timeout * 1_000)
        )

    async def wait_for_invisibility_css(
        self, selector: str, timeout: float = 5
    ) -> None:
        await self.__page.wait_for_selector(
            selector, state="hidden", timeout=int(timeout * 1_000)
        )

    async def wait_and_send_keys_xpath(
        self, xpath: str, text: str, timeout: float = 5
    ) -> None:
        locator = self.__page.locator(f"xpath={xpath}")
        await locator.wait_for(timeout=int(timeout * 1_000))
        await locator.fill(text)

    async def wait_and_send_keys_css(
        self, selector: str, text: str, timeout: float = 5
    ) -> None:
        locator = self.__page.locator(selector)
        await locator.wait_for(timeout=int(timeout * 1_000))
        await locator.fill(text)

    async def close(self) -> None:
        try:
            await self.__page.close()
        finally:
            try:
                await self.__browser.close()
            finally:
                await self.__playwright.stop()

    async def quit(self) -> None:
        await self.close()
