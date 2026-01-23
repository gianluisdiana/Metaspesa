import { Product, ShoppingList } from '@/lib/domain';

export default interface ApiService {
  getCurrentShoppingList(): Promise<ShoppingList>;
  getRegisteredProducts(): Promise<Product[]>;
  recordShoppingList(shoppingList: ShoppingList): Promise<void>;
}
