import type { RegisteredItemsResponse__Output } from '@/generated-protos/Metaspesa/Protos/Shopping/RegisteredItemsResponse';
import type { ShoppingListResponse__Output } from '@/generated-protos/Metaspesa/Protos/Shopping/ShoppingListResponse';
import type { ShoppingListSummariesResponse__Output } from '@/generated-protos/Metaspesa/Protos/Shopping/ShoppingListSummariesResponse';
import type { ShoppingServiceClient } from '@/generated-protos/shopping/ShoppingService';
import * as grpc from '@grpc/grpc-js';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import GrpcApiService from '@/infrastructure/grpc-api-service';
import type { GrpcClientFactory } from '@/infrastructure/grpc-client-factory';

const { emitMock } = vi.hoisted(() => ({
  emitMock: vi.fn(),
}));

vi.mock('server-only', () => ({}));

vi.mock('@opentelemetry/api-logs', () => ({
  SeverityNumber: { ERROR: 17 },
  logs: {
    getLogger: () => ({ emit: emitMock }),
  },
}));

function unaryCall(): grpc.ClientUnaryCall {
  return {} as grpc.ClientUnaryCall;
}

function callWithError<T>(
  error: grpc.ServiceError,
): (
  request: unknown,
  metadata: grpc.Metadata,
  callback: grpc.requestCallback<T>,
) => grpc.ClientUnaryCall {
  return (_request, _metadata, callback) => {
    callback(error);
    return unaryCall();
  };
}

function callWithResponse<T>(
  response: T,
): (
  request: unknown,
  metadata: grpc.Metadata,
  callback: grpc.requestCallback<T>,
) => grpc.ClientUnaryCall {
  return (_request, _metadata, callback) => {
    callback(null, response);
    return unaryCall();
  };
}

function createService(client: Record<string, unknown>): GrpcApiService {
  return new GrpcApiService('token', {
    createAuthorizedMetadata: () => new grpc.Metadata(),
    createShoppingServiceClient: () =>
      client as unknown as ShoppingServiceClient,
  } as unknown as GrpcClientFactory);
}

function serviceError(): grpc.ServiceError {
  return Object.assign(new Error('backend unavailable'), {
    code: grpc.status.UNAVAILABLE,
    details: 'backend unavailable',
    metadata: new grpc.Metadata(),
  });
}

describe('GrpcApiService observability', () => {
  beforeEach(() => {
    emitMock.mockClear();
  });

  it('logs and rethrows shopping list read failures', async () => {
    const error = serviceError();
    const service = createService({
      GetShoppingList: vi.fn(callWithError(error)),
    });

    await expect(service.getShoppingList()).rejects.toBe(error);

    expect(emitMock).toHaveBeenCalledWith(
      expect.objectContaining({
        attributes: expect.objectContaining({
          'error.message': 'backend unavailable',
          'grpc.code': grpc.status.UNAVAILABLE,
          'grpc.method': 'GetShoppingList',
          'grpc.service': 'ShoppingService',
        }),
        severityText: 'ERROR',
      }),
    );
  });

  it('logs and rethrows shopping list summary read failures', async () => {
    const error = serviceError();
    const service = createService({
      GetShoppingListSummaries: vi.fn(callWithError(error)),
    });

    await expect(service.getShoppingListSummaries()).rejects.toBe(error);

    expect(emitMock).toHaveBeenCalledWith(
      expect.objectContaining({
        attributes: expect.objectContaining({
          'error.message': 'backend unavailable',
          'grpc.code': grpc.status.UNAVAILABLE,
          'grpc.method': 'GetShoppingListSummaries',
          'grpc.service': 'ShoppingService',
        }),
        severityText: 'ERROR',
      }),
    );
  });

  it('returns successful empty shopping lists without logging', async () => {
    const service = createService({
      GetShoppingList: vi.fn(
        callWithResponse<ShoppingListResponse__Output>({
          shoppingList: {
            items: [],
            name: '',
          },
        }),
      ),
    });

    await expect(service.getShoppingList()).resolves.toEqual({
      name: '',
      products: [],
    });
    expect(emitMock).not.toHaveBeenCalled();
  });

  it('returns successful empty shopping summaries without logging', async () => {
    const service = createService({
      GetShoppingListSummaries: vi.fn(
        callWithResponse<ShoppingListSummariesResponse__Output>({
          shoppingLists: [],
        }),
      ),
    });

    await expect(service.getShoppingListSummaries()).resolves.toEqual([]);
    expect(emitMock).not.toHaveBeenCalled();
  });
});
