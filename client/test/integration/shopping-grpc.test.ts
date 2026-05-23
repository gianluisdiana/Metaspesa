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

async function createApiServiceWithTemporaryShoppingList() {
  const loginResponse = await registerAndLogin();
  const service = new GrpcApiService(loginResponse.token);
  await service.createShoppingList();

  return service;
}

const milk = {
  checked: false,
  name: 'Integration Milk',
  price: 1.29,
  quantity: '1 l',
};

const bread = {
  checked: false,
  name: 'Integration Bread',
  price: 2.49,
  quantity: '1 unit',
};

describeIfGrpc('shopping gRPC integration', () => {
  it('creates a temporary shopping list with empty name', async () => {
    const { createResponse } = await createTemporaryShoppingList();

    expect(createResponse.name).toBe('');
  });

  it('returns created temporary shopping list in summaries', async () => {
    const { summaries } = await createTemporaryShoppingList();

    expect(
      summaries.shoppingLists.filter(summary => summary.name === ''),
    ).toHaveLength(1);
  });

  it('adds a shopping item through api service', async () => {
    const service = await createApiServiceWithTemporaryShoppingList();

    await service.addItemsToList(undefined, [milk]);

    await expect(service.getShoppingList()).resolves.toEqual(
      expect.objectContaining({
        products: expect.arrayContaining([expect.objectContaining(milk)]),
      }),
    );
  });

  it('updates only the requested shopping item fields through api service', async () => {
    const service = await createApiServiceWithTemporaryShoppingList();
    await service.addItemsToList(undefined, [milk]);

    await service.updateItem(undefined, 'Integration Milk', { checked: true });

    await expect(service.getShoppingList()).resolves.toEqual(
      expect.objectContaining({
        products: expect.arrayContaining([
          expect.objectContaining({
            checked: true,
            name: 'Integration Milk',
            price: 1.29,
            quantity: '1 l',
          }),
        ]),
      }),
    );
  });

  it('removes one shopping item and preserves remaining items through api service', async () => {
    const service = await createApiServiceWithTemporaryShoppingList();
    await service.addItemsToList(undefined, [milk, bread]);

    await service.removeItem(undefined, 'Integration Milk');

    await expect(service.getShoppingList()).resolves.toEqual(
      expect.objectContaining({
        products: [bread],
      }),
    );
  });
});
