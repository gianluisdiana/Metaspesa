import type { CreateShoppingListResponse__Output } from '@/generated-protos/Metaspesa/Protos/Shopping/CreateShoppingListResponse';
import type { ShoppingListSummariesResponse__Output } from '@/generated-protos/Metaspesa/Protos/Shopping/ShoppingListSummariesResponse';
import { expect, it, vi } from 'vitest';

import GrpcApiService from '@/infrastructure/grpc-api-service';

import {
  authMetadata,
  createShoppingClient,
  describeIfGrpc,
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

const milk = {
  checked: false,
  name: 'Integration Milk',
  price: 1.29,
  productFormatUid: 1,
  quantity: '1 l',
};

const bread = {
  checked: false,
  name: 'Integration Bread',
  price: 2.49,
  productFormatUid: 2,
  quantity: '1 unit',
};

describeIfGrpc('shopping gRPC integration', () => {
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
    const service = await createApiServiceWithShoppingList();

    await service.addItemsToList(undefined, [milk]);

    await expect(service.getShoppingList()).resolves.toEqual(
      expect.objectContaining({
        products: expect.arrayContaining([expect.objectContaining(milk)]),
      }),
    );
  });

  it('updates only the requested shopping item fields through api service', async () => {
    const shoppingListName = uniqueListName('Integration Update');
    const service = await createApiServiceWithShoppingList(shoppingListName);
    await service.addItemsToList(shoppingListName, [milk]);

    await service.updateItem(shoppingListName, milk.productFormatUid, {
      checked: true,
    });

    await expect(service.getShoppingList(shoppingListName)).resolves.toEqual(
      expect.objectContaining({
        products: expect.arrayContaining([
          expect.objectContaining({
            checked: true,
            name: milk.name,
            price: 1.29,
            quantity: '1 l',
          }),
        ]),
      }),
    );
  });

  it('removes one shopping item and preserves remaining items through api service', async () => {
    const shoppingListName = uniqueListName('Integration Remove');
    const service = await createApiServiceWithShoppingList(shoppingListName);
    await service.addItemsToList(shoppingListName, [milk, bread]);

    await service.removeItem(shoppingListName, milk.productFormatUid);

    await expect(service.getShoppingList(shoppingListName)).resolves.toEqual(
      expect.objectContaining({
        products: [bread],
      }),
    );
  });

  it('records checked items and resets their checked state through api service', async () => {
    const shoppingListName = uniqueListName('Integration Record');
    const service = await createApiServiceWithShoppingList(shoppingListName);
    await service.addItemsToList(shoppingListName, [milk]);
    await service.updateItem(shoppingListName, milk.productFormatUid, {
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
