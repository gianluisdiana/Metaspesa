import type { GetMarketProductsResponse__Output } from '@/generated-protos/Metaspesa/Protos/Markets/GetMarketProductsResponse';
import type { CreateShoppingListResponse__Output } from '@/generated-protos/Metaspesa/Protos/Shopping/CreateShoppingListResponse';
import type { ShoppingListSummariesResponse__Output } from '@/generated-protos/Metaspesa/Protos/Shopping/ShoppingListSummariesResponse';
import type { ServiceError } from '@grpc/grpc-js';
import { expect, it, vi } from 'vitest';

import GrpcApiService from '@/infrastructure/grpc-api-service';
import type { ProductMessage } from '@/lib/shopping-list-contracts';

import {
  authMetadata,
  createMarketClient,
  createShoppingClient,
  describeIfApis,
  registerAndLogin,
  requireResponse,
} from './grpc-test-client';

vi.mock('server-only', () => ({}));

async function createTemporaryShoppingList() {
  const loginResponse = await registerAndLogin();
  const metadata = authMetadata(loginResponse.token);
  const shoppingClient = createShoppingClient();
  const createResponse = await new Promise<CreateShoppingListResponse__Output>(
    (resolve, reject) => {
      shoppingClient.CreateShoppingList({}, metadata, (err, response) => {
        if (err) {
          reject(err);
          return;
        }
        resolve(requireResponse(response, 'CreateShoppingList'));
      });
    },
  );
  const summaries = await new Promise<ShoppingListSummariesResponse__Output>(
    (resolve, reject) => {
      shoppingClient.GetShoppingListSummaries({}, metadata, (err, response) => {
        if (err) {
          reject(err);
          return;
        }
        resolve(requireResponse(response, 'GetShoppingListSummaries'));
      });
    },
  );

  return { createResponse, summaries };
}

async function createApiServiceWithShoppingList(name?: string) {
  const loginResponse = await registerAndLogin();
  const service = new GrpcApiService(loginResponse.token);
  await service.createShoppingList(name);

  return service;
}

function uniqueListName(prefix: string): string {
  return `${prefix} ${Date.now()}`;
}

async function getCatalogProducts(count: number): Promise<ProductMessage[]> {
  const response = await new Promise<GetMarketProductsResponse__Output>(
    (resolve, reject) => {
      createMarketClient().GetMarketProducts(
        { page: 1, pageSize: count },
        (
          error: ServiceError | null,
          value?: GetMarketProductsResponse__Output,
        ) => {
          if (error) {
            reject(error);
            return;
          }
          resolve(requireResponse(value, 'GetMarketProducts'));
        },
      );
    },
  );
  const products = response.markets.flatMap(market =>
    market.products.flatMap(product =>
      product.formats.map(format => ({
        checked: false,
        name: product.name,
        price: Number(format.price),
        productFormatUid: format.productFormatUid,
        quantity: format.quantity,
      })),
    ),
  );
  if (products.length < count) {
    throw new Error(
      `Shopping integration tests require ${count} catalog products.`,
    );
  }
  return products.slice(0, count);
}

describeIfApis('shopping gRPC integration with REST identity', () => {
  it('creates a temporary shopping list with empty name', async () => {
    const { createResponse } = await createTemporaryShoppingList();

    expect(createResponse.name).toBeUndefined();
  });

  it('returns created temporary shopping list in summaries', async () => {
    const { summaries } = await createTemporaryShoppingList();

    expect(
      summaries.shoppingLists.filter(summary => summary.name === undefined),
    ).toHaveLength(1);
  });

  it('adds a shopping item through api service', async () => {
    const [product] = await getCatalogProducts(1);
    const service = await createApiServiceWithShoppingList();

    await service.addItemsToList(undefined, [product]);

    await expect(service.getShoppingList()).resolves.toEqual(
      expect.objectContaining({
        products: expect.arrayContaining([expect.objectContaining(product)]),
      }),
    );
  });

  it('updates only the requested shopping item fields through api service', async () => {
    const [product] = await getCatalogProducts(1);
    const shoppingListName = uniqueListName('Integration Update');
    const service = await createApiServiceWithShoppingList(shoppingListName);
    await service.addItemsToList(shoppingListName, [product]);

    await service.updateItem(shoppingListName, product.productFormatUid, {
      checked: true,
    });

    await expect(service.getShoppingList(shoppingListName)).resolves.toEqual(
      expect.objectContaining({
        products: expect.arrayContaining([
          expect.objectContaining({
            checked: true,
            name: product.name,
            price: product.price,
            quantity: product.quantity,
          }),
        ]),
      }),
    );
  });

  it('removes one shopping item and preserves remaining items through api service', async () => {
    const [removedProduct, retainedProduct] = await getCatalogProducts(2);
    const shoppingListName = uniqueListName('Integration Remove');
    const service = await createApiServiceWithShoppingList(shoppingListName);
    await service.addItemsToList(shoppingListName, [
      removedProduct,
      retainedProduct,
    ]);

    await service.removeItem(shoppingListName, removedProduct.productFormatUid);

    await expect(service.getShoppingList(shoppingListName)).resolves.toEqual(
      expect.objectContaining({
        products: [retainedProduct],
      }),
    );
  });

  it('records checked items and resets their checked state through api service', async () => {
    const [product] = await getCatalogProducts(1);
    const shoppingListName = uniqueListName('Integration Record');
    const service = await createApiServiceWithShoppingList(shoppingListName);
    await service.addItemsToList(shoppingListName, [product]);
    await service.updateItem(shoppingListName, product.productFormatUid, {
      checked: true,
    });

    await service.recordShoppingList(shoppingListName);

    await expect(service.getShoppingList(shoppingListName)).resolves.toEqual(
      expect.objectContaining({
        products: [expect.objectContaining({ checked: false })],
      }),
    );
  });
});
