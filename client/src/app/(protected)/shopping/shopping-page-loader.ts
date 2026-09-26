import RestShoppingApiService from '@/infrastructure/rest-shopping-api-service';
import {
  ShoppingListMessage,
  ShoppingListSummaryMessage,
} from '@/lib/shopping-list-contracts';

export type ShoppingPageData = {
  selectedListId?: string;
  shoppingList: ShoppingListMessage;
  shoppingListSummaries: ShoppingListSummaryMessage[];
};

const emptyShoppingList: ShoppingListMessage = { products: [] };

export async function loadShoppingPage(
  service: Pick<
    RestShoppingApiService,
    'getShoppingList' | 'getShoppingListSummaries'
  >,
  requestedListId?: string,
): Promise<ShoppingPageData> {
  const shoppingListSummaries = await service.getShoppingListSummaries();
  const selectedListId = selectListId(shoppingListSummaries, requestedListId);

  if (shoppingListSummaries.length === 0) {
    return {
      selectedListId,
      shoppingList: emptyShoppingList,
      shoppingListSummaries,
    };
  }

  return {
    selectedListId,
    shoppingList: await service.getShoppingList(selectedListId!),
    shoppingListSummaries,
  };
}

function selectListId(
  summaries: ShoppingListSummaryMessage[],
  requestedListId?: string,
): string | undefined {
  if (
    requestedListId &&
    summaries.some(summary => summary.id === requestedListId)
  ) {
    return requestedListId;
  }

  const temporaryList = summaries.find(summary => summary.isTemporary);
  return temporaryList?.id ?? summaries[0]?.id;
}
