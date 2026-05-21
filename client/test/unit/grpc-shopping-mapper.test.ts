import { describe, expect, it } from 'vitest';

import { GrpcShoppingMapper } from '@/infrastructure/grpc-shopping-mapper';

describe('GrpcShoppingMapper', () => {
  const mapper = new GrpcShoppingMapper();

  it('fails on missing shopping lists', () => {
    expect(() => mapper.mapShoppingList()).toThrow(
      'Malformed gRPC response: ShoppingList',
    );
  });

  it('maps shopping items with rounded prices', () => {
    expect(
      mapper.mapShoppingList({
        items: [
          {
            checked: false,
            name: 'Milk',
            price: '1.235',
            quantity: '1 liter',
          },
        ],
        name: 'Groceries',
      }),
    ).toEqual({
      name: 'Groceries',
      products: [
        {
          checked: false,
          name: 'Milk',
          price: 1.24,
          quantity: '1 liter',
        },
      ],
    });
  });

  it('maps missing registered items to an empty collection', () => {
    expect(mapper.mapRegisteredItems()).toEqual([]);
  });

  it('maps shopping list summaries', () => {
    expect(mapper.mapShoppingListSummaries([{ name: 'Groceries' }])).toEqual([
      { name: 'Groceries' },
    ]);
  });
});
