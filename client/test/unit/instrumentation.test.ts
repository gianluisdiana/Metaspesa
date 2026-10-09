import { register } from '@/instrumentation';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

const telemetry = vi.hoisted(() => ({ startedEndpoints: [] as string[] }));

vi.mock('@opentelemetry/sdk-node', () => ({
  NodeSDK: class {
    private readonly endpoint: string;

    constructor(options: { traceExporter: { url: string } }) {
      this.endpoint = options.traceExporter.url;
    }

    start() {
      telemetry.startedEndpoints.push(this.endpoint);
    }
  },
}));
vi.mock('@opentelemetry/auto-instrumentations-node', () => ({
  getNodeAutoInstrumentations: () => [],
}));
vi.mock('@opentelemetry/exporter-trace-otlp-grpc', () => ({
  OTLPTraceExporter: class {
    readonly url: string;

    constructor(options: { url: string }) {
      this.url = options.url;
    }
  },
}));
vi.mock('@opentelemetry/exporter-metrics-otlp-grpc', () => ({
  OTLPMetricExporter: class {},
}));
vi.mock('@opentelemetry/exporter-logs-otlp-grpc', () => ({
  OTLPLogExporter: class {},
}));
vi.mock('@opentelemetry/sdk-metrics', () => ({
  PeriodicExportingMetricReader: class {},
}));
vi.mock('@opentelemetry/sdk-logs', () => ({
  BatchLogRecordProcessor: class {},
}));
vi.mock('@opentelemetry/resources', () => ({
  resourceFromAttributes: (attributes: object) => attributes,
}));
vi.mock('@opentelemetry/semantic-conventions', () => ({
  ATTR_SERVICE_NAME: 'service.name',
}));

describe('client telemetry opt-in', () => {
  afterEach(() => vi.unstubAllEnvs());

  beforeEach(() => {
    vi.unstubAllEnvs();
    vi.stubEnv('NEXT_RUNTIME', 'nodejs');
    telemetry.startedEndpoints.length = 0;
  });

  it.each([undefined, '', '   '])(
    'does not start export when the endpoint is %j',
    async endpoint => {
      vi.stubEnv('OTEL_EXPORTER_OTLP_ENDPOINT', endpoint);

      await register();

      expect(telemetry.startedEndpoints).toEqual([]);
    },
  );

  it('starts export to the configured endpoint', async () => {
    vi.stubEnv('OTEL_EXPORTER_OTLP_ENDPOINT', ' https://alloy:4317 ');

    await register();

    expect(telemetry.startedEndpoints).toEqual(['https://alloy:4317']);
  });

  it('does not start Node telemetry in the edge runtime', async () => {
    vi.stubEnv('NEXT_RUNTIME', 'edge');
    vi.stubEnv('OTEL_EXPORTER_OTLP_ENDPOINT', 'https://alloy:4317');

    await register();

    expect(telemetry.startedEndpoints).toEqual([]);
  });
});
