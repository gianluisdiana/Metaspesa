from contextlib import contextmanager

from opentelemetry import metrics, trace


class ScraperTelemetry:
    def __init__(self, tracer: trace.Tracer, meter: metrics.Meter) -> None:
        self.__tracer = tracer

    @contextmanager
    def measure_run(self):
        with self.__tracer.start_as_current_span("scrape_all_markets"):
            yield
