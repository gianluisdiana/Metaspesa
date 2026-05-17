import {
  ProductMessage,
  ShoppingListMessage,
  ShoppingListSummaryMessage,
} from './messages';

export default interface ApiService {
  createShoppingList(name?: string): Promise<void>;
  getShoppingList(name?: string): Promise<ShoppingListMessage>;
  getShoppingListSummaries(): Promise<ShoppingListSummaryMessage[]>;
  getRegisteredProducts(): Promise<ProductMessage[]>;
  recordShoppingList(shoppingList: ShoppingListMessage): Promise<void>;
}
