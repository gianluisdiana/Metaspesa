import {
  ProductMessage,
  ShoppingItemUpdateMessage,
  ShoppingListMessage,
  ShoppingListSummaryMessage,
  ShoppingListUpdateMessage,
} from './shopping-list-contracts';

export default interface ApiService {
  addItemsToList(
    shoppingListName: string | undefined,
    products: ProductMessage[],
  ): Promise<void>;
  createShoppingList(name?: string): Promise<void>;
  getShoppingList(name?: string): Promise<ShoppingListMessage>;
  getShoppingListSummaries(): Promise<ShoppingListSummaryMessage[]>;
  removeItem(
    shoppingListName: string | undefined,
    productFormatUid: number,
  ): Promise<void>;
  recordShoppingList(shoppingListName?: string): Promise<void>;
  updateItem(
    shoppingListName: string | undefined,
    productFormatUid: number,
    update: ShoppingItemUpdateMessage,
  ): Promise<void>;
  updateShoppingList(
    shoppingListName: string | undefined,
    update: ShoppingListUpdateMessage,
  ): Promise<void>;
}
