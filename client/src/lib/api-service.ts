import { ProductMessage, ShoppingListMessage } from './messages';

export default interface ApiService {
  createShoppingList(name?: string): Promise<void>;
  getShoppingList(name?: string): Promise<ShoppingListMessage>;
  getRegisteredProducts(): Promise<ProductMessage[]>;
  recordShoppingList(shoppingList: ShoppingListMessage): Promise<void>;
}
