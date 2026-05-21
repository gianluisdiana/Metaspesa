import type { CreateShoppingListResponse__Output } from '@/generated-protos/Metaspesa/Protos/Shopping/CreateShoppingListResponse';
import type { ShoppingListSummariesResponse__Output } from '@/generated-protos/Metaspesa/Protos/Shopping/ShoppingListSummariesResponse';
import { expect, it } from 'vitest';

import {
  authMetadata,
  createShoppingClient,
  describeIfGrpc,
  registerAndLogin,
  requireResponse,
} from './grpc-test-client';

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

describeIfGrpc('shopping gRPC integration', () => {
  it('creates a temporary shopping list with empty name', async () => {
    const { createResponse } = await createTemporaryShoppingList();

    expect(createResponse.name).toBe('');
  });

  it('returns created temporary shopping list in summaries', async () => {
    const { summaries } = await createTemporaryShoppingList();

    expect(summaries.shoppingLists).toEqual(
      expect.arrayContaining([expect.objectContaining({ name: '' })]),
    );
  });
});
