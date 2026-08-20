import type { ShoppingServiceClient } from '@/generated-protos/shopping/ShoppingService';
import * as grpc from '@grpc/grpc-js';
import { describe, expect, it, vi } from 'vitest';

import GrpcApiService from '@/infrastructure/grpc-api-service';
import type { GrpcClientFactory } from '@/infrastructure/grpc-client-factory';

vi.mock('server-only', () => ({}));

function unaryCall(): grpc.ClientUnaryCall {
  return {} as grpc.ClientUnaryCall;
}

function createService(client: Record<string, unknown>): GrpcApiService {
  return new GrpcApiService('token', {
    createAuthorizedMetadata: () => new grpc.Metadata(),
    createShoppingServiceClient: () =>
      client as unknown as ShoppingServiceClient,
  } as unknown as GrpcClientFactory);
}

describe('GrpcApiService', () => {
  it('records a named shopping list by name', async () => {
    const recordShoppingList = vi.fn(
      (
        _request: unknown,
        _metadata: grpc.Metadata,
        callback: grpc.requestCallback<unknown>,
      ) => {
        callback(null, {});
        return unaryCall();
      },
    );
    const service = createService({ RecordShoppingList: recordShoppingList });

    await service.recordShoppingList('Weekly');

    expect(recordShoppingList).toHaveBeenCalledWith(
      { shoppingListName: 'Weekly' },
      expect.any(grpc.Metadata),
      expect.any(Function),
    );
  });

  it('records the temporary shopping list with an empty name', async () => {
    const recordShoppingList = vi.fn(
      (
        _request: unknown,
        _metadata: grpc.Metadata,
        callback: grpc.requestCallback<unknown>,
      ) => {
        callback(null, {});
        return unaryCall();
      },
    );
    const service = createService({ RecordShoppingList: recordShoppingList });

    await service.recordShoppingList();

    expect(recordShoppingList).toHaveBeenCalledWith(
      { shoppingListName: '' },
      expect.any(grpc.Metadata),
      expect.any(Function),
    );
  });

  it('rejects when recording fails', async () => {
    const error: grpc.ServiceError = Object.assign(new Error('record failed'), {
      code: grpc.status.INTERNAL,
      details: 'record failed',
      metadata: new grpc.Metadata(),
    });
    const service = createService({
      RecordShoppingList: vi.fn(
        (
          _request: unknown,
          _metadata: grpc.Metadata,
          callback: grpc.requestCallback<unknown>,
        ) => {
          callback(error);
          return unaryCall();
        },
      ),
    });

    await expect(service.recordShoppingList('Weekly')).rejects.toBe(error);
  });
});
