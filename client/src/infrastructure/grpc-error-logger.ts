import { SeverityNumber, logs } from '@opentelemetry/api-logs';

type GrpcErrorLogOptions = {
  error: unknown;
  grpcMethod: string;
  grpcService: string;
  loggerName: string;
};

function errorRecord(error: unknown): Record<string, unknown> {
  return error && typeof error === 'object'
    ? (error as Record<string, unknown>)
    : {};
}

function errorMessage(error: unknown): string {
  if (error instanceof Error) return error.message;
  if (typeof error === 'string') return error;

  return 'Unknown gRPC error';
}

export function logGrpcReadFailure({
  error,
  grpcMethod,
  grpcService,
  loggerName,
}: GrpcErrorLogOptions): void {
  const record = errorRecord(error);
  const attributes: Record<string, string | number | boolean> = {
    'error.message': errorMessage(error),
    'grpc.method': grpcMethod,
    'grpc.service': grpcService,
  };

  if (typeof record.code === 'number') {
    attributes['grpc.code'] = record.code;
  }

  if (typeof record.name === 'string') {
    attributes['error.name'] = record.name;
  }

  logs.getLogger(loggerName).emit({
    attributes,
    body: `${grpcService}.${grpcMethod} failed: ${errorMessage(error)}`,
    severityNumber: SeverityNumber.ERROR,
    severityText: 'ERROR',
  });
}
