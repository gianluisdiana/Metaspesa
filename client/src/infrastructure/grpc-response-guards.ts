export class MalformedGrpcResponseError extends Error {
  public constructor(responseName: string) {
    super(`Malformed gRPC response: ${responseName}`);
    this.name = 'MalformedGrpcResponseError';
  }
}

export function requireGrpcResponse<T>(
  response: T | null | undefined,
  responseName: string,
): T {
  if (response === null || response === undefined) {
    throw new MalformedGrpcResponseError(responseName);
  }

  return response;
}
