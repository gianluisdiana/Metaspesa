import 'server-only';

import * as grpc from '@grpc/grpc-js';
import { context, propagation } from '@opentelemetry/api';

export function createTracingMetadata(): grpc.Metadata {
  const metadata = new grpc.Metadata();
  const carrier: Record<string, string> = {};
  propagation.inject(context.active(), carrier);
  Object.entries(carrier).forEach(([key, value]) => metadata.set(key, value));
  return metadata;
}
