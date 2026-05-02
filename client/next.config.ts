import type { NextConfig } from 'next';

const nextConfig: NextConfig = {
  serverExternalPackages: [
    '@opentelemetry/sdk-node',
    '@opentelemetry/auto-instrumentations-node',
    '@opentelemetry/exporter-trace-otlp-grpc',
    '@opentelemetry/exporter-metrics-otlp-grpc',
  ],
};

export default nextConfig;
