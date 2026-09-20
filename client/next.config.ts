import type { NextConfig } from 'next';

const nextConfig: NextConfig = {
  images: {
    remotePatterns: [
      {
        hostname: 'lh3.googleusercontent.com',
        protocol: 'https',
      },
      {
        hostname: 'www.alcampo.es',
        protocol: 'https',
      },
      {
        hostname: 'prod-mercadona.imgix.net',
        protocol: 'https',
      },
    ],
  },
  serverExternalPackages: [
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
