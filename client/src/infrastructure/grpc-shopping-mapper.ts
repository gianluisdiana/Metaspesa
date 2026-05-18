import { dollars } from '@/lib/formatters/money-formatter';
import {
  ProductMessage,
  ShoppingListMessage,
  ShoppingListSummaryMessage,
} from '@/lib/shopping-list-contracts';

import { RegisteredItemsResponse__Output } from '@/protos/shopping/RegisteredItemsResponse';
import { ShoppingItem__Output } from '@/protos/shopping/ShoppingItem';
import { ShoppingList__Output } from '@/protos/shopping/ShoppingList';
import { ShoppingListSummary__Output } from '@/protos/shopping/ShoppingListSummary';

export class GrpcShoppingMapper {
  public mapRegisteredItems(
    response?: RegisteredItemsResponse__Output,
  ): ProductMessage[] {
    return response?.items?.map(item => this.mapShoppingItem(item)) ?? [];
  }

  public mapShoppingList(
    proto?: ShoppingList__Output | null,
  ): ShoppingListMessage {
    return {
      name: proto?.name ?? '',
      products: proto?.items?.map(item => this.mapShoppingItem(item)) ?? [],
    };
  }

  public mapShoppingListSummaries(
    summaries?: ShoppingListSummary__Output[],
  ): ShoppingListSummaryMessage[] {
    return (
      summaries?.map(summary => this.mapShoppingListSummary(summary)) ?? []
    );
  }

  public mapShoppingListSummary(
    proto: ShoppingListSummary__Output,
  ): ShoppingListSummaryMessage {
    return { name: proto.name };
  }

  private mapShoppingItem(item: ShoppingItem__Output): ProductMessage {
    return {
      checked: item.checked ?? false,
      name: item.name,
      price:
        item.price === undefined
          ? undefined
          : dollars.round(Number(item.price)),
      quantity: item.quantity,
    };
  }
}
