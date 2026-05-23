import {
  ProductMessage,
  ShoppingItemUpdateMessage,
  ShoppingListMessage,
  ShoppingListSummaryMessage,
} from './shopping-list-contracts';

export default interface ApiService {
  addItemsToList(
    shoppingListName: string | undefined,
    products: ProductMessage[],
  ): Promise<void>;
  createShoppingList(name?: string): Promise<void>;
  getShoppingList(name?: string): Promise<ShoppingListMessage>;
  getShoppingListSummaries(): Promise<ShoppingListSummaryMessage[]>;
  getRegisteredProducts(): Promise<ProductMessage[]>;
  removeItem(
    shoppingListName: string | undefined,
    itemName: string,
  ): Promise<void>;
  recordShoppingList(shoppingList: ShoppingListMessage): Promise<void>;
  updateItem(
    shoppingListName: string | undefined,
    itemName: string,
    update: ShoppingItemUpdateMessage,
  ): Promise<void>;
}
