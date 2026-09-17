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

async function getMarketProducts(
  nameSegment?: string,
): Promise<GetMarketProductsResponse__Output> {
  return await new Promise<GetMarketProductsResponse__Output>(
    (resolve, reject) => {
      createMarketClient().GetMarketProducts(
        { nameSegment, page: 1, pageSize: 20 },
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

function catalogRows(response: GetMarketProductsResponse__Output) {
  const rows = [];
  for (const market of response.markets) {
    for (const product of market.products) {
      for (const format of product.formats) {
        rows.push({
          brand: product.brandName,
          market: market.name,
          name: product.name,
          price: Number(format.price),
          quantity: format.quantity,
        });
      }
    }
  }
  return rows;
}

describeIfGrpc('market gRPC integration', () => {
  it('connects to public markets endpoint', async () => {
    const response = await getMarkets();

    expect(response.markets.map(market => market.name).sort()).toEqual([
      'Integration Market A',
      'Integration Market B',
    ]);
  });

  it('connects to public market products endpoint', async () => {
    const response = await getMarketProducts('milk');

    expect(response.totalProducts).toBe(1);
    expect(response.markets).toEqual([
      expect.objectContaining({
        name: 'Integration Market A',
        products: [
          expect.objectContaining({
            brandName: 'Integration Brand',
            formats: [
              expect.objectContaining({
                imageUrl: '',
                price: '1.25',
                productFormatUid: expect.any(Number),
                quantity: '1 l',
              }),
            ],
            name: 'Integration Milk 1L',
          }),
        ],
      }),
    ]);
  });

  it('returns all seeded products with known formats and prices', async () => {
    const response = await getMarketProducts();
    const expectedProducts = [
      {
        brand: 'Integration Brand',
        market: 'Integration Market A',
        name: 'Integration Bread',
        price: 2.1,
        quantity: '1 unit',
      },
      {
        brand: 'Integration Brand',
        market: 'Integration Market A',
        name: 'Integration Milk 1L',
        price: 1.25,
        quantity: '1 l',
      },
      {
        brand: 'Integration Brand',
        market: 'Integration Market B',
        name: 'Integration Pasta',
        price: 3.4,
        quantity: '1 kg',
      },
    ];

    expect(response.totalProducts).toBe(expectedProducts.length);
    expect(
      catalogRows(response).sort((left, right) =>
        left.name.localeCompare(right.name),
      ),
    ).toEqual(expectedProducts);
  });
});
