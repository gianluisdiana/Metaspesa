import type { GetMarketProductsResponse__Output } from '@/generated-protos/Metaspesa/Protos/Markets/GetMarketProductsResponse';
import type { GetMarketsResponse__Output } from '@/generated-protos/Metaspesa/Protos/Markets/GetMarketsResponse';
import { expect, it } from 'vitest';

import {
  createMarketClient,
  describeIfGrpc,
  requireResponse,
} from './grpc-test-client';

async function getMarkets(): Promise<GetMarketsResponse__Output> {
  return await new Promise<GetMarketsResponse__Output>((resolve, reject) => {
    createMarketClient().GetMarkets({}, (err, response) => {
      if (err) {
        reject(err);
        return;
      }
      resolve(requireResponse(response, 'GetMarkets'));
    });
  });
}

async function getMarketProducts(): Promise<GetMarketProductsResponse__Output> {
  return await new Promise<GetMarketProductsResponse__Output>(
    (resolve, reject) => {
      createMarketClient().GetMarketProducts(
        { nameSegment: 'milk', page: 1, pageSize: 20 },
        (err, response) => {
          if (err) {
            reject(err);
            return;
          }
          resolve(requireResponse(response, 'GetMarketProducts'));
        },
      );
    },
  );
}

describeIfGrpc('market gRPC integration', () => {
  it('connects to public markets endpoint', async () => {
    const response = await getMarkets();

    expect(response.markets).toBeDefined();
  });

  it('connects to public market products endpoint', async () => {
    const response = await getMarketProducts();

    expect(response.totalProducts).toBeGreaterThanOrEqual(0);
  });
});
