import path from 'node:path';

import type { LoginResponse__Output } from '@/generated-protos/Metaspesa/Protos/Auth/LoginResponse';
import type { AuthServiceClient } from '@/generated-protos/auth/AuthService';
import type { ProtoGrpcType as AuthProtoGrpcType } from '@/generated-protos/auth_service';
import type { MarketServiceClient } from '@/generated-protos/markets/MarketService';
import type { ProtoGrpcType as MarketsProtoGrpcType } from '@/generated-protos/markets_service';
import type { ShoppingServiceClient } from '@/generated-protos/shopping/ShoppingService';
import type { ProtoGrpcType as ShoppingProtoGrpcType } from '@/generated-protos/shopping_service';
import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';
import { describe } from 'vitest';

export const grpcServerUrl = process.env.GRPC_SERVER_URL;
export const describeIfGrpc = grpcServerUrl ? describe : describe.skip;
export const password = 'SecurePass1!';

function loadPackage<T>(protoPath: string, options?: protoLoader.Options): T {
  const packageDefinition = protoLoader.loadSync(
    path.resolve(process.cwd(), protoPath),
    {
      includeDirs: [path.resolve(process.cwd(), 'src/infrastructure')],
      ...options,
    },
  );
  return grpc.loadPackageDefinition(packageDefinition) as unknown as T;
}

export function requireResponse<T>(response: T | undefined, name: string): T {
  if (!response) {
    throw new Error(`${name} did not return a response.`);
  }
  return response;
}

export function createAuthClient(): AuthServiceClient {
  const { AuthService } = loadPackage<AuthProtoGrpcType>(
    'src/infrastructure/protos/Auth/auth_service.proto',
  ).Metaspesa.Protos.Auth;

  return new AuthService(grpcServerUrl!, grpc.credentials.createInsecure());
}

export function createMarketClient(): MarketServiceClient {
  const { MarketService } = loadPackage<MarketsProtoGrpcType>(
    'src/infrastructure/protos/Markets/markets_service.proto',
  ).Metaspesa.Protos.Markets;

  return new MarketService(grpcServerUrl!, grpc.credentials.createInsecure());
}

export function createShoppingClient(): ShoppingServiceClient {
  const { ShoppingService } = loadPackage<ShoppingProtoGrpcType>(
    'src/infrastructure/protos/Shopping/shopping_service.proto',
    { defaults: true },
  ).Metaspesa.Protos.Shopping;

  return new ShoppingService(grpcServerUrl!, grpc.credentials.createInsecure());
}

export async function registerAndLogin(): Promise<LoginResponse__Output> {
  const authClient = createAuthClient();
  const credentials = {
    password,
    username: `client_it_${Date.now()}`,
  };

  await new Promise<void>((resolve, reject) => {
    authClient.Register(credentials, err => {
      if (err) {
        reject(err);
        return;
      }
      resolve();
    });
  });

  return await new Promise<LoginResponse__Output>((resolve, reject) => {
    authClient.Login(credentials, (err, response) => {
      if (err) {
        reject(err);
        return;
      }
      resolve(requireResponse(response, 'Login'));
    });
  });
}

export function authMetadata(token: string): grpc.Metadata {
  const metadata = new grpc.Metadata();
  metadata.set('Authorization', `Bearer ${token}`);
  return metadata;
}
