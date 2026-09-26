import { loadShoppingPage } from '@/app/(protected)/shopping/shopping-page-loader';
import { describe, expect, it, vi } from 'vitest';

import RestShoppingApiService from '@/infrastructure/rest-shopping-api-service';

function createService({
  lists,
  shoppingList,
}: {
  lists: { id: string; name?: string; isTemporary: boolean }[];
  shoppingList?: { name?: string; products: [] };
}) {
  return {
    getShoppingList: vi.fn().mockResolvedValue(shoppingList),
    getShoppingListSummaries: vi.fn().mockResolvedValue(lists),
  } satisfies Pick<
    RestShoppingApiService,
    'getShoppingList' | 'getShoppingListSummaries'
  >;
}

describe('shopping page loader', () => {
  it('loads the first available named list when no list is selected', async () => {
    const groceries = { name: 'Groceries', products: [] as [] };
    const service = createService({
      lists: [
        { id: '7', isTemporary: false, name: 'Groceries' },
        { id: '8', isTemporary: false, name: 'Weekly' },
      ],
      shoppingList: groceries,
    });

    await expect(loadShoppingPage(service)).resolves.toEqual({
      selectedListId: '7',
      shoppingList: groceries,
      shoppingListSummaries: [
        { id: '7', isTemporary: false, name: 'Groceries' },
        { id: '8', isTemporary: false, name: 'Weekly' },
      ],
    });
  });

  it('loads the temporary list by default when one is available', async () => {
    const temporaryList = { products: [] as [] };
    const service = createService({
      lists: [
        { id: '7', isTemporary: false, name: 'Groceries' },
        { id: '9', isTemporary: true },
      ],
      shoppingList: temporaryList,
    });

    await expect(loadShoppingPage(service)).resolves.toEqual({
      selectedListId: '9',
      shoppingList: temporaryList,
      shoppingListSummaries: [
        { id: '7', isTemporary: false, name: 'Groceries' },
        { id: '9', isTemporary: true },
      ],
    });
  });

  it('loads the requested named list', async () => {
    const requestedListId = '8';
    const weekly = { name: 'Weekly', products: [] as [] };
    const service = createService({
      lists: [
        { id: '7', isTemporary: false, name: 'Groceries' },
        { id: requestedListId, isTemporary: false, name: 'Weekly' },
      ],
      shoppingList: weekly,
    });

    await expect(loadShoppingPage(service, requestedListId)).resolves.toEqual({
      selectedListId: requestedListId,
      shoppingList: weekly,
      shoppingListSummaries: [
        { id: '7', isTemporary: false, name: 'Groceries' },
        { id: requestedListId, isTemporary: false, name: 'Weekly' },
      ],
    });
  });

  it('returns an empty page model when no shopping lists exist', async () => {
    const service = createService({ lists: [] });

    await expect(loadShoppingPage(service)).resolves.toEqual({
      selectedListId: undefined,
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
