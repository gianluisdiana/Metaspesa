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
  outputFileTracingIncludes: {
    '/*': ['src/infrastructure/protos/**/*.proto'],
  },
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
  turbopack: {
    ignoreIssue: [
      {
        path: 'src/infrastructure/grpc-client-factory.ts',
        title: /^TP1105 /,
      },
    ],
  },
};

export default nextConfig;
