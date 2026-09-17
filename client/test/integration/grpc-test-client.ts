import { randomUUID } from 'node:crypto';
import path from 'node:path';

import type { MarketServiceClient } from '@/generated-protos/markets/MarketService';
import type { ProtoGrpcType as MarketsProtoGrpcType } from '@/generated-protos/markets_service';
import type { ShoppingServiceClient } from '@/generated-protos/shopping/ShoppingService';
import type { ProtoGrpcType as ShoppingProtoGrpcType } from '@/generated-protos/shopping_service';
import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';
import { describe } from 'vitest';

export const grpcServerUrl = process.env.GRPC_SERVER_URL;
export const restApiUrl = process.env.REST_API_URL;
export const describeIfGrpc = grpcServerUrl ? describe : describe.skip;
export const describeIfRest = restApiUrl ? describe : describe.skip;
export const describeIfApis =
  grpcServerUrl && restApiUrl ? describe : describe.skip;
export const password = 'SecurePass1!';

function loadPackage<T>(protoPath: string, options?: protoLoader.Options): T {
  const resolvedProtoPath = path.resolve(process.cwd(), protoPath);
  const packageDefinition = protoLoader.loadSync(resolvedProtoPath, {
    includeDirs: [
      path.dirname(resolvedProtoPath),
      path.resolve(process.cwd(), 'src/infrastructure'),
    ],
    ...options,
  });
  return grpc.loadPackageDefinition(packageDefinition) as unknown as T;
}

export function requireResponse<T>(response: T | undefined, name: string): T {
  if (!response) {
    throw new Error(`${name} did not return a response.`);
  }
  return response;
}

export function createMarketClient(): MarketServiceClient {
  const { MarketService } = loadPackage<MarketsProtoGrpcType>(
    'src/infrastructure/protos/Markets/markets_service.proto',
    { defaults: true },
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

export async function registerAndLogin(): Promise<{
  expirationInUtc: string;
  token: string;
}> {
  const credentials = {
    password,
    username: `client_it_${randomUUID()}`,
  };

  const registration = await fetch(`${restApiUrl}/auth/registrations`, {
    body: JSON.stringify(credentials),
    headers: {
      'Content-Type': 'application/json',
      Origin: 'http://localhost:3000',
    },
    method: 'POST',
  });
  if (!registration.ok) {
    throw new Error(`Registration failed with ${registration.status}.`);
  }

  const session = await fetch(`${restApiUrl}/auth/sessions`, {
    body: JSON.stringify(credentials),
    headers: {
      'Content-Type': 'application/json',
      Origin: 'http://localhost:3000',
    },
    method: 'POST',
  });
  if (!session.ok) {
    throw new Error(`Login failed with ${session.status}.`);
  }
  const cookie = session.headers.get('set-cookie');
  const cookieParts = cookie?.split(';') ?? [];
  const [sessionPart] = cookieParts;
  const token = sessionPart?.startsWith('metaspesa_session=')
    ? sessionPart.slice('metaspesa_session='.length)
    : undefined;
  const expirationInUtc = cookieParts
    .find(part => part.trimStart().toLowerCase().startsWith('expires='))
    ?.trimStart()
    .slice('expires='.length);
  if (!token || !expirationInUtc) {
    throw new Error('Login did not return the session cookie.');
  }
  return { expirationInUtc, token };
}

export function authMetadata(token: string): grpc.Metadata {
  const metadata = new grpc.Metadata();
  metadata.set('Authorization', `Bearer ${token}`);
  return metadata;
}
