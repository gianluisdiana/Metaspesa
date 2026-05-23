import {
  ProductMessage,
  ShoppingItemUpdateMessage,
  ShoppingListMessage,
  ShoppingListSummaryMessage,
} from '@/lib/shopping-list-contracts';

export type CreateListResponse = {
  message?: string;
  shoppingList: ShoppingListMessage;
  shoppingListSummaries: ShoppingListSummaryMessage[];
};

export class ShoppingListClient {
  public async addItemsToList(
    shoppingListName: string | undefined,
    items: ProductMessage[],
  ): Promise<CreateListResponse> {
    return await this.mutateItems('POST', {
      items,
      shoppingListName,
    });
  }

  public async createTemporaryList(): Promise<CreateListResponse> {
    const response = await fetch('/api/shopping/lists', {
      method: 'POST',
    });
    const body = (await response.json()) as CreateListResponse;
    if (!response.ok) {
      throw new Error(body.message ?? 'Could not create a temporary list.');
    }

    return body;
  }

  public async getShoppingList(name?: string): Promise<ShoppingListMessage> {
    const params = new URLSearchParams();
    params.set('name', name ?? '');
    const response = await fetch(`/api/shopping/lists?${params}`, {
      cache: 'no-store',
    });
    if (!response.ok) {
      throw new Error('Could not load the current shopping list.');
    }

    return (await response.json()) as ShoppingListMessage;
  }

  public async getShoppingListSummaries(): Promise<
    ShoppingListSummaryMessage[]
  > {
    const response = await fetch('/api/shopping/lists', {
      cache: 'no-store',
    });
    if (!response.ok) {
      throw new Error('Could not load shopping lists.');
    }

    return (await response.json()) as ShoppingListSummaryMessage[];
  }

  public async removeItem(
    shoppingListName: string | undefined,
    itemName: string,
  ): Promise<CreateListResponse> {
    return await this.mutateItems('DELETE', {
      itemName,
      shoppingListName,
    });
  }

  public async updateItem(
    shoppingListName: string | undefined,
    itemName: string,
    update: ShoppingItemUpdateMessage,
  ): Promise<CreateListResponse> {
    return await this.mutateItems('PATCH', {
      itemName,
      shoppingListName,
      update,
    });
  }

  private async mutateItems(
    method: 'DELETE' | 'PATCH' | 'POST',
    body: unknown,
  ): Promise<CreateListResponse> {
    const response = await fetch('/api/shopping/lists/items', {
      body: JSON.stringify(body),
      headers: { 'Content-Type': 'application/json' },
      method,
    });
    const responseBody = (await response.json()) as CreateListResponse;
    if (!response.ok) {
      throw new Error(
        responseBody.message ?? 'Could not update shopping list.',
      );
    }

    return responseBody;
  }
}
