import 'server-only';

export class GrpcConfig {
  public constructor(
    public readonly serverUrl: string,
    public readonly backendSecure: boolean,
  ) {}

  public static fromEnvironment(): GrpcConfig {
    const serverUrl = process.env.GRPC_SERVER_URL;
    if (!serverUrl) {
      throw new Error('GRPC_SERVER_URL must be configured.');
    }

    return new GrpcConfig(serverUrl, process.env.BACKEND_SECURE === 'true');
  }
}
