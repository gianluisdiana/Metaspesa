import { ProductMessage, ShoppingListMessage } from './messages';

export default interface ApiService {
  getCurrentShoppingList(): Promise<ShoppingListMessage>;
  getRegisteredProducts(): Promise<ProductMessage[]>;
  recordShoppingList(shoppingList: ShoppingListMessage): Promise<void>;
}
