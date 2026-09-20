from datetime import date
from typing import override

from application.abstractions import Clock


class SystemClock(Clock):
    @override
    def today(self) -> date:
        return date.today()
