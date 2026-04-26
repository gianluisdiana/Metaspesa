from abc import ABC, abstractmethod


class WebDriver(ABC):
    @abstractmethod
    async def get(self, url: str) -> None: ...

    @abstractmethod
    async def refresh(self) -> None: ...

    @property
    @abstractmethod
    def current_url(self) -> str: ...

    @abstractmethod
    async def page_source(self) -> str: ...

    @abstractmethod
    async def execute_script(self, script: str): ...

    @abstractmethod
    async def wait_and_click_css(self, selector: str, timeout: float = 5) -> None: ...

    @abstractmethod
    async def wait_and_click_xpath(self, xpath: str, timeout: float = 5) -> None: ...

    @abstractmethod
    async def wait_for_presence_css(
        self, selector: str, timeout: float = 5
    ) -> None: ...

    @abstractmethod
    async def wait_for_presence_xpath(self, xpath: str, timeout: float = 5) -> None: ...

    @abstractmethod
    async def wait_for_invisibility_css(
        self, selector: str, timeout: float = 5
    ) -> None: ...

    @abstractmethod
    async def wait_and_send_keys_xpath(
        self, xpath: str, text: str, timeout: float = 5
    ) -> None: ...

    @abstractmethod
    async def wait_and_send_keys_css(
        self, selector: str, text: str, timeout: float = 5
    ) -> None: ...

    @abstractmethod
    async def close(self) -> None: ...

    @abstractmethod
    async def quit(self) -> None: ...
