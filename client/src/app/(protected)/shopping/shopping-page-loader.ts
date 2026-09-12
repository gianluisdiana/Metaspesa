import ApiService from '@/lib/api-service';
import {
  ShoppingListMessage,
  ShoppingListSummaryMessage,
} from '@/lib/shopping-list-contracts';

export type ShoppingPageData = {
  selectedListName?: string;
  shoppingList: ShoppingListMessage;
  shoppingListSummaries: ShoppingListSummaryMessage[];
};

const emptyShoppingList: ShoppingListMessage = { products: [] };

export async function loadShoppingPage(
  service: Pick<ApiService, 'getShoppingList' | 'getShoppingListSummaries'>,
  requestedListName?: string,
): Promise<ShoppingPageData> {
  const shoppingListSummaries = await service.getShoppingListSummaries();
  const selectedListName = selectListName(
    shoppingListSummaries,
    requestedListName,
  );

  if (shoppingListSummaries.length === 0) {
    return {
      selectedListName,
      shoppingList: emptyShoppingList,
      shoppingListSummaries,
    };
  }

  return {
    selectedListName,
    shoppingList: await service.getShoppingList(selectedListName),
    shoppingListSummaries,
  };
}

function selectListName(
  summaries: ShoppingListSummaryMessage[],
  requestedListName?: string,
): string | undefined {
  if (requestedListName) {
    return requestedListName;
  }

  const temporaryList = summaries.find(summary => !summary.name);
  return temporaryList ? undefined : summaries[0]?.name;
}
