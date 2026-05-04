import type { NextConfig } from 'next';

const nextConfig: NextConfig = {
  serverExternalPackages: [
    '@grpc/grpc-js',
    '@grpc/proto-loader',
    '@opentelemetry/api',
    '@opentelemetry/api-logs',
    '@opentelemetry/auto-instrumentations-node',
    '@opentelemetry/exporter-logs-otlp-grpc',
    '@opentelemetry/exporter-metrics-otlp-grpc',
    '@opentelemetry/exporter-trace-otlp-grpc',
    '@opentelemetry/sdk-logs',
    '@opentelemetry/sdk-node',
  ],
};

export default nextConfig;
