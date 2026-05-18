import {
  ProductMessage,
  ShoppingListMessage,
  ShoppingListSummaryMessage,
} from './shopping-list-contracts';

export default interface ApiService {
  createShoppingList(name?: string): Promise<void>;
  getShoppingList(name?: string): Promise<ShoppingListMessage>;
  getShoppingListSummaries(): Promise<ShoppingListSummaryMessage[]>;
  getRegisteredProducts(): Promise<ProductMessage[]>;
  recordShoppingList(shoppingList: ShoppingListMessage): Promise<void>;
}
