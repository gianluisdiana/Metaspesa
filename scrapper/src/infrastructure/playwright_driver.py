from playwright.sync_api import Browser, Page, Playwright, sync_playwright


class PlaywrightDriver:
    def __init__(self, headless: bool = True) -> None:
        self.__playwright: Playwright = sync_playwright().start()
        self.__browser: Browser = self.__playwright.firefox.launch(headless=headless)
        self.__page: Page = self.__browser.new_page()

    def get(self, url: str) -> None:
        self.__page.goto(url)

    def refresh(self) -> None:
        self.__page.reload()

    @property
    def current_url(self) -> str:
        return self.__page.url

    @property
    def page_source(self) -> str:
        return self.__page.content()

    def execute_script(self, script: str):
        return self.__page.evaluate(script)

    def wait_and_click_css(self, selector: str, timeout: float = 5) -> None:
        self.__page.wait_for_selector(selector, timeout=int(timeout * 1_000))
        self.__page.locator(selector).click()

    def wait_and_click_xpath(self, xpath: str, timeout: float = 5) -> None:
        self.__page.wait_for_selector(f"xpath={xpath}", timeout=int(timeout * 1_000))
        self.__page.locator(f"xpath={xpath}").click()

    def wait_for_presence_css(self, selector: str, timeout: float = 5) -> None:
        self.__page.wait_for_selector(selector, timeout=int(timeout * 1_000))

    def wait_for_presence_xpath(self, xpath: str, timeout: float = 5) -> None:
        self.__page.wait_for_selector(f"xpath={xpath}", timeout=int(timeout * 1_000))

    def wait_for_invisibility_css(self, selector: str, timeout: float = 5) -> None:
        self.__page.wait_for_selector(
            selector, state="hidden", timeout=int(timeout * 1_000)
        )

    def wait_and_send_keys_xpath(
        self, xpath: str, text: str, timeout: float = 5
    ) -> None:
        locator = self.__page.locator(f"xpath={xpath}")
        locator.wait_for(timeout=int(timeout * 1_000))
        locator.fill(text)

    def wait_and_send_keys_css(
        self, selector: str, text: str, timeout: float = 5
    ) -> None:
        locator = self.__page.locator(selector)
        locator.wait_for(timeout=int(timeout * 1_000))
        locator.fill(text)

    def close(self) -> None:
        try:
            if self.__page:
                self.__page.close()
        finally:
            try:
                self.__browser.close()
            finally:
                self.__playwright.stop()

    # Convenience alias used by main
    def quit(self) -> None:
        self.close()
