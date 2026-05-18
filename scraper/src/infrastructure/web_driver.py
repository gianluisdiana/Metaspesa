from abc import ABC, abstractmethod
from dataclasses import dataclass
from typing import Literal


@dataclass
class Selector:
    target: str
    type: Literal["css", "xpath"] = "css"


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
    async def execute_script(self, script: str) -> None: ...

    @abstractmethod
    async def wait_and_click(self, selector: Selector, timeout: float = 5) -> None: ...

    @abstractmethod
    async def wait_for_presence(
        self, selector: Selector, timeout: float = 5
    ) -> None: ...

    @abstractmethod
    async def wait_for_invisibility_css(
        self, selector: str, timeout: float = 5
    ) -> None: ...

    @abstractmethod
    async def wait_and_send_keys(
        self, selector: Selector, text: str, timeout: float = 5
    ) -> None: ...

    @abstractmethod
    async def close(self) -> None: ...

    @abstractmethod
    async def quit(self) -> None: ...
