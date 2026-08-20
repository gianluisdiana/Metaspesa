import { afterEach, describe, expect, it, vi } from 'vitest';

import { ShoppingListClient } from '@/lib/shopping-list';

const ProductFormatUid = 10;

function shoppingListResponse(body: unknown, ok = true) {
  return {
    json: () => Promise.resolve(body),
    ok,
  };
}

function successfulCreateListResponse() {
  return {
    message: 'Shopping list updated.',
    shoppingList: { name: 'Groceries', products: [] },
    shoppingListSummaries: [{ name: 'Groceries' }],
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

  it('throws response message when naming a temporary list fails', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValue(
        shoppingListResponse({ message: 'Name already exists.' }, false),
      );
    vi.stubGlobal('fetch', fetchMock);
    const client = new ShoppingListClient();

    await expect(
      client.nameTemporaryListAndCreateNew('Groceries'),
    ).rejects.toThrow('Name already exists.');
  });

  it('sends a post request when adding items to a named list', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValue(shoppingListResponse(successfulCreateListResponse()));
    vi.stubGlobal('fetch', fetchMock);
    const client = new ShoppingListClient();

    await client.addItemsToList('Groceries', [
      {
        checked: false,
        name: 'Milk',
        price: 1.29,
        productFormatUid: ProductFormatUid,
        quantity: '1 l',
      },
    ]);

    expect(fetchMock).toHaveBeenCalledWith('/api/shopping/lists/items', {
      body: JSON.stringify({
        items: [
          {
            checked: false,
            name: 'Milk',
            price: 1.29,
            productFormatUid: ProductFormatUid,
            quantity: '1 l',
          },
        ],
        shoppingListName: 'Groceries',
      }),
      headers: { 'Content-Type': 'application/json' },
      method: 'POST',
    });
  });

  it('sends a patch request when updating a list item', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValue(shoppingListResponse(successfulCreateListResponse()));
    vi.stubGlobal('fetch', fetchMock);
    const client = new ShoppingListClient();

    await client.updateItem('Groceries', ProductFormatUid, { checked: true });

    expect(fetchMock).toHaveBeenCalledWith('/api/shopping/lists/items', {
      body: JSON.stringify({
        productFormatUid: ProductFormatUid,
        shoppingListName: 'Groceries',
        update: { checked: true },
      }),
      headers: { 'Content-Type': 'application/json' },
      method: 'PATCH',
    });
  });

  it('sends a delete request when removing a list item', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValue(shoppingListResponse(successfulCreateListResponse()));
    vi.stubGlobal('fetch', fetchMock);
    const client = new ShoppingListClient();

    await client.removeItem('Groceries', ProductFormatUid);

    expect(fetchMock).toHaveBeenCalledWith('/api/shopping/lists/items', {
      body: JSON.stringify({
        productFormatUid: ProductFormatUid,
        shoppingListName: 'Groceries',
      }),
      headers: { 'Content-Type': 'application/json' },
      method: 'DELETE',
    });
  });

  it('throws default message when item mutation fails without response message', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValue(shoppingListResponse({}, false));
    vi.stubGlobal('fetch', fetchMock);
    const client = new ShoppingListClient();

    await expect(
      client.removeItem('Groceries', ProductFormatUid),
    ).rejects.toThrow('Could not update shopping list.');
  });
});
