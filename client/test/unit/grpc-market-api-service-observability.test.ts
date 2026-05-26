import type { GetMarketProductsResponse__Output } from '@/generated-protos/Metaspesa/Protos/Markets/GetMarketProductsResponse';
import type { GetMarketsResponse__Output } from '@/generated-protos/Metaspesa/Protos/Markets/GetMarketsResponse';
import type { MarketServiceClient } from '@/generated-protos/markets/MarketService';
import * as grpc from '@grpc/grpc-js';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import type { GrpcClientFactory } from '@/infrastructure/grpc-client-factory';
import GrpcMarketApiService from '@/infrastructure/grpc-market-api-service';

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

function createService(client: Record<string, unknown>): GrpcMarketApiService {
  return new GrpcMarketApiService('token', {
    createAuthorizedMetadata: () => new grpc.Metadata(),
    createMarketServiceClient: () => client as unknown as MarketServiceClient,
  } as unknown as GrpcClientFactory);
}

function serviceError(): grpc.ServiceError {
  return Object.assign(new Error('backend unavailable'), {
    code: grpc.status.UNAVAILABLE,
    details: 'backend unavailable',
    metadata: new grpc.Metadata(),
  });
}

describe('GrpcMarketApiService observability', () => {
  beforeEach(() => {
    emitMock.mockClear();
  });

  it('logs and rethrows market product read failures', async () => {
    const error = serviceError();
    const service = createService({
      GetMarketProducts: vi.fn(callWithError(error)),
    });

    await expect(
      service.getMarketProducts({ page: 1, pageSize: 20 }),
    ).rejects.toBe(error);

    expect(emitMock).toHaveBeenCalledWith(
      expect.objectContaining({
        attributes: expect.objectContaining({
          'error.message': 'backend unavailable',
          'grpc.code': grpc.status.UNAVAILABLE,
          'grpc.method': 'GetMarketProducts',
          'grpc.service': 'MarketService',
        }),
        severityText: 'ERROR',
      }),
    );
  });

  it('logs and rethrows market summary read failures', async () => {
    const error = serviceError();
    const service = createService({
      GetMarkets: vi.fn(callWithError(error)),
    });

    await expect(service.getMarkets()).rejects.toBe(error);

    expect(emitMock).toHaveBeenCalledWith(
      expect.objectContaining({
        attributes: expect.objectContaining({
          'error.message': 'backend unavailable',
          'grpc.code': grpc.status.UNAVAILABLE,
          'grpc.method': 'GetMarkets',
          'grpc.service': 'MarketService',
        }),
        severityText: 'ERROR',
      }),
    );
  });

  it('returns successful empty market products without logging', async () => {
    const service = createService({
      GetMarketProducts: vi.fn(
        callWithResponse<GetMarketProductsResponse__Output>({
          markets: [],
          totalProducts: 0,
        }),
      ),
    });

    await expect(
      service.getMarketProducts({ page: 1, pageSize: 20 }),
    ).resolves.toEqual({ markets: [], totalProducts: 0 });
    expect(emitMock).not.toHaveBeenCalled();
  });

  it('returns successful empty market summaries without logging', async () => {
    const service = createService({
      GetMarkets: vi.fn(
        callWithResponse<GetMarketsResponse__Output>({
          markets: [],
        }),
      ),
    });

    await expect(service.getMarkets()).resolves.toEqual([]);
    expect(emitMock).not.toHaveBeenCalled();
  });
});
