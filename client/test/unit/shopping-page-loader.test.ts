import { loadShoppingPage } from '@/app/(protected)/shopping/shopping-page-loader';
import { describe, expect, it, vi } from 'vitest';

import ApiService from '@/lib/api-service';

function createService({
  lists,
  shoppingList,
}: {
  lists: { name?: string }[];
  shoppingList?: { name?: string; products: [] };
}) {
  return {
    getShoppingList: vi.fn().mockResolvedValue(shoppingList),
    getShoppingListSummaries: vi.fn().mockResolvedValue(lists),
  } satisfies Pick<ApiService, 'getShoppingList' | 'getShoppingListSummaries'>;
}

describe('shopping page loader', () => {
  it('loads the first available named list when no list is selected', async () => {
    const groceries = { name: 'Groceries', products: [] as [] };
    const service = createService({
      lists: [{ name: 'Groceries' }, { name: 'Weekly' }],
      shoppingList: groceries,
    });

    await expect(loadShoppingPage(service)).resolves.toEqual({
      selectedListName: 'Groceries',
      shoppingList: groceries,
      shoppingListSummaries: [{ name: 'Groceries' }, { name: 'Weekly' }],
    });
  });

  it('loads the temporary list by default when one is available', async () => {
    const temporaryList = { products: [] as [] };
    const service = createService({
      lists: [{ name: 'Groceries' }, {}],
      shoppingList: temporaryList,
    });

    await expect(loadShoppingPage(service)).resolves.toEqual({
      selectedListName: undefined,
      shoppingList: temporaryList,
      shoppingListSummaries: [{ name: 'Groceries' }, {}],
    });
  });

  it('loads the requested named list', async () => {
    const weekly = { name: 'Weekly', products: [] as [] };
    const service = createService({
      lists: [{ name: 'Groceries' }, { name: 'Weekly' }],
      shoppingList: weekly,
    });

    await expect(loadShoppingPage(service, 'Weekly')).resolves.toEqual({
      selectedListName: 'Weekly',
      shoppingList: weekly,
      shoppingListSummaries: [{ name: 'Groceries' }, { name: 'Weekly' }],
    });
  });

  it('returns an empty page model when no shopping lists exist', async () => {
    const service = createService({ lists: [] });

    await expect(loadShoppingPage(service)).resolves.toEqual({
      selectedListName: undefined,
      shoppingList: { products: [] },
      shoppingListSummaries: [],
    });
  });

  it('does not request missing shopping-list details when no lists exist', async () => {
    const service = createService({ lists: [] });

    await loadShoppingPage(service);

    expect(service.getShoppingList).not.toHaveBeenCalled();
  });
});
