import { afterEach, describe, expect, it, vi } from 'vitest';

import { ShoppingListClient } from '@/lib/shopping-list';

function shoppingListResponse(body: unknown, ok = true) {
  return {
    json: () => Promise.resolve(body),
    ok,
  };
}

describe('ShoppingListClient', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('sends a patch request when naming a temporary list before creating a new one', async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      shoppingListResponse({
        message: 'Temporary list created.',
        shoppingList: { name: undefined, products: [] },
        shoppingListSummaries: [{ name: 'Groceries' }, { name: undefined }],
      }),
    );
    vi.stubGlobal('fetch', fetchMock);
    const client = new ShoppingListClient();

    await client.nameTemporaryListAndCreateNew('Groceries');

    expect(fetchMock).toHaveBeenCalledWith('/api/shopping/lists', {
      body: JSON.stringify({
        update: { name: 'Groceries' },
      }),
      headers: { 'Content-Type': 'application/json' },
      method: 'PATCH',
    });
  });
});
