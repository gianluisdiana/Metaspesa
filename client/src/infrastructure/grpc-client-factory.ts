import 'server-only';

import path from 'node:path';

import { ShoppingServiceClient } from '@/generated-protos/shopping/ShoppingService';
import { ProtoGrpcType as ShoppingProtoGrpcType } from '@/generated-protos/shopping_service';
import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';

import { GrpcConfig } from './grpc-config';
import { createTracingMetadata } from './grpc-metadata';

export class GrpcClientFactory {
  public constructor(private readonly config = GrpcConfig.fromEnvironment()) {}

  public createAuthorizedMetadata(token: string): grpc.Metadata {
    const metadata = createTracingMetadata();
    metadata.set('Authorization', `Bearer ${token}`);
    return metadata;
  }

  public createMetadata(): grpc.Metadata {
    return createTracingMetadata();
  }

  public createShoppingServiceClient(): ShoppingServiceClient {
    const definition = protoLoader.loadSync(
      path.join(
        process.cwd(),
        'src/infrastructure/protos/Shopping/shopping_service.proto',
      ),
      { defaults: true },
    );
    const { ShoppingService } =
      this.loadPackage<ShoppingProtoGrpcType>(definition).Metaspesa.Protos
        .Shopping;

    return new ShoppingService(this.config.serverUrl, this.credentials);
  }

  private get credentials(): grpc.ChannelCredentials {
    return this.config.backendSecure
      ? grpc.credentials.createSsl()
      : grpc.credentials.createInsecure();
  }

  private loadPackage<T>(definition: protoLoader.PackageDefinition): T {
    return grpc.loadPackageDefinition(definition) as unknown as T;
  }
}
