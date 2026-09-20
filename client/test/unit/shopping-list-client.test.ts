import { describe, expect, it, vi } from 'vitest';

import RestShoppingApiService from '@/infrastructure/rest-shopping-api-service';

const httpStatus = { conflict: 409, ok: 200 } as const;
const namedListId = 7;
const productFormatId = 93;

function jsonResponse(body: unknown, status: number = httpStatus.ok): Response {
  return new Response(JSON.stringify(body), {
    headers: { 'Content-Type': 'application/json' },
    status,
  });
}

describe('REST shopping client', () => {
  it('reads stable list IDs from summaries', async () => {
    const fetcher = vi.fn().mockResolvedValue(
      jsonResponse({
        items: [
          { id: namedListId, isTemporary: false, name: 'Weekly' },
          { id: 9, isTemporary: true },
        ],
      }),
    );
    const client = new RestShoppingApiService(
      undefined,
      'http://api.test/api/v1',
      fetcher,
    );

    await expect(client.getShoppingListSummaries()).resolves.toEqual([
      { id: namedListId, isTemporary: false, name: 'Weekly' },
      { id: 9, isTemporary: true, name: undefined },
    ]);
  });

  it('maps REST item amount into displayed total price', async () => {
    const fetcher = vi.fn().mockResolvedValue(
      jsonResponse({
        id: namedListId,
        isTemporary: false,
        items: [
          {
            amount: 2,
            brand: 'Brand',
            checked: true,
            market: { id: 1, name: 'Market' },
            productFormatId,
            productName: 'Milk',
            quantity: { amount: 1, unit: 'l' },
            unitPrice: { amount: 1.25, currency: 'EUR' },
          },
        ],
        name: 'Weekly',
      }),
    );
    const client = new RestShoppingApiService(
      undefined,
      'http://api.test/api/v1',
      fetcher,
    );

    await expect(client.getShoppingList(namedListId)).resolves.toEqual({
      id: namedListId,
      name: 'Weekly',
      products: [
        {
          checked: true,
          name: 'Milk',
          price: 2.5,
          productFormatUid: productFormatId,
          quantity: '2 × 1 l',
        },
      ],
    });
  });

  it('posts only REST item fields and accepts empty 204 response', async () => {
    const fetcher = vi
      .fn()
      .mockResolvedValue(new Response(null, { status: 204 }));
    const client = new RestShoppingApiService(
      undefined,
      'http://api.test/api/v1',
      fetcher,
    );

    await client.addItemsToList(namedListId, [
      { checked: false, productFormatUid: productFormatId },
    ]);

    expect(fetcher).toHaveBeenCalledWith(
      'http://api.test/api/v1/shopping-lists/7/items',
      expect.objectContaining({
        body: JSON.stringify({
          items: [{ amount: 1, checked: false, productFormatId }],
        }),
        method: 'POST',
      }),
    );
  });

  it('uses ID in item patch path', async () => {
    const fetcher = vi
      .fn()
      .mockResolvedValue(new Response(null, { status: 204 }));
    const client = new RestShoppingApiService(
      undefined,
      'http://api.test/api/v1',
      fetcher,
    );

    await client.updateItem(namedListId, productFormatId, { checked: true });

    expect(fetcher).toHaveBeenCalledWith(
      'http://api.test/api/v1/shopping-lists/7/items/93',
      expect.objectContaining({ body: '{"checked":true}', method: 'PATCH' }),
    );
  });

  it('exposes problem code for duplicate temporary list', async () => {
    const fetcher = vi.fn().mockResolvedValue(
      jsonResponse(
        {
          code: 'ShoppingList.Temporary.AlreadyExists',
          title: 'Temporary shopping list already exists',
        },
        httpStatus.conflict,
      ),
    );
    const client = new RestShoppingApiService(
      undefined,
      'http://api.test/api/v1',
      fetcher,
    );

    await expect(client.createShoppingList()).rejects.toMatchObject({
      code: 'ShoppingList.Temporary.AlreadyExists',
      status: httpStatus.conflict,
    });
  });

  it('sends server render token as bearer authentication', async () => {
    const fetcher = vi.fn().mockResolvedValue(jsonResponse({ items: [] }));
    const client = new RestShoppingApiService(
      'server-token',
      'http://api.test/api/v1',
      fetcher,
    );

    await client.getShoppingListSummaries();

    expect(fetcher).toHaveBeenCalledWith(
      'http://api.test/api/v1/shopping-lists',
      expect.objectContaining({
        headers: expect.objectContaining({
          Authorization: 'Bearer server-token',
        }),
      }),
    );
  });
});
