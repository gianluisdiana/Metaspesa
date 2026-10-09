import pytest

from infrastructure.telemetry import otel
from infrastructure.telemetry.scraper_telemetry import ScraperTelemetry


@pytest.mark.parametrize("endpoint", [None, "", "   "])
def test_unconfigured_telemetry_works_without_constructing_exporters(
    monkeypatch, endpoint
):
    def forbid_exporter(*args, **kwargs):
        raise AssertionError("Unconfigured telemetry must not create an exporter")

    monkeypatch.setattr(otel, "OTLPSpanExporter", forbid_exporter)
    monkeypatch.setattr(otel, "OTLPMetricExporter", forbid_exporter)
    monkeypatch.setattr(otel, "OTLPLogExporter", forbid_exporter)

    telemetry = otel.setup_telemetry(endpoint)

    assert isinstance(telemetry, ScraperTelemetry)


def test_configured_endpoint_is_normalized_before_exporter_creation(monkeypatch):
    def observe_endpoint(*, endpoint):
        # Stop at the transport boundary, before registering global SDK providers.
        raise RuntimeError(endpoint)

    monkeypatch.setattr(otel, "OTLPSpanExporter", observe_endpoint)

    with pytest.raises(RuntimeError, match=r"^https://alloy:4317$"):
        otel.setup_telemetry(" https://alloy:4317 ")
