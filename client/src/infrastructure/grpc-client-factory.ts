import 'server-only';

import path from 'node:path';

import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';

import { AuthServiceClient } from '@/protos/auth/AuthService';
import { ProtoGrpcType as AuthProtoGrpcType } from '@/protos/auth_service';
import { MarketServiceClient } from '@/protos/markets/MarketService';
import { ProtoGrpcType as MarketsProtoGrpcType } from '@/protos/markets_service';
import { ShoppingServiceClient } from '@/protos/shopping/ShoppingService';
import { ProtoGrpcType as ShoppingProtoGrpcType } from '@/protos/shopping_service';

import { GrpcConfig } from './grpc-config';
import { createTracingMetadata } from './grpc-metadata';

export class GrpcClientFactory {
  public constructor(private readonly config = GrpcConfig.fromEnvironment()) {}

  public createAuthorizedMetadata(token: string): grpc.Metadata {
    const metadata = createTracingMetadata();
    metadata.set('Authorization', `Bearer ${token}`);
    return metadata;
  }

  public createAuthServiceClient(): AuthServiceClient {
    const { AuthService } = this.loadPackage<AuthProtoGrpcType>(
      'src/infrastructure/protos/Auth/auth_service.proto',
    ).Metaspesa.Protos.Auth;

    return new AuthService(this.config.serverUrl, this.credentials);
  }

  public createMetadata(): grpc.Metadata {
    return createTracingMetadata();
  }

  public createMarketServiceClient(): MarketServiceClient {
    const { MarketService } = this.loadPackage<MarketsProtoGrpcType>(
      'src/infrastructure/protos/Markets/markets_service.proto',
    ).Metaspesa.Protos.Markets;

    return new MarketService(this.config.serverUrl, this.credentials);
  }

  public createShoppingServiceClient(): ShoppingServiceClient {
    const { ShoppingService } = this.loadPackage<ShoppingProtoGrpcType>(
      'src/infrastructure/protos/Shopping/shopping_service.proto',
      { defaults: true },
    ).Metaspesa.Protos.Shopping;

    return new ShoppingService(this.config.serverUrl, this.credentials);
  }

  private get credentials(): grpc.ChannelCredentials {
    return this.config.backendSecure
      ? grpc.credentials.createSsl()
      : grpc.credentials.createInsecure();
  }

  private loadPackage<T>(protoPath: string, options?: protoLoader.Options): T {
    const packageDefinition = protoLoader.loadSync(
      path.resolve(process.cwd(), protoPath),
      options,
    );
    return grpc.loadPackageDefinition(packageDefinition) as unknown as T;
  }
}
