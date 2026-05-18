import { describe, expect, it } from 'vitest';

import { GrpcShoppingMapper } from '@/infrastructure/grpc-shopping-mapper';

describe('GrpcShoppingMapper', () => {
  const mapper = new GrpcShoppingMapper();

  it('maps missing shopping list to an empty list', () => {
    expect(mapper.mapShoppingList()).toEqual({
      name: '',
      products: [],
    });
  });

  it('maps shopping items with rounded prices and default checked state', () => {
    expect(
      mapper.mapShoppingList({
        items: [
          {
            checked: undefined,
            name: 'Milk',
            price: 1.235,
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
