from abc import ABC, abstractmethod
from datetime import date
from typing import override


class Clock(ABC):
    @abstractmethod
    def today(self) -> date: ...


class SystemClock(Clock):
    @override
    def today(self) -> date:
        return date.today()
