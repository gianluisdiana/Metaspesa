import ApiService from '@/lib/api-service';
import {
  ProductMessage,
  ShoppingListMessage,
  ShoppingListSummaryMessage,
} from '@/lib/shopping-list-contracts';

export default class FakeApiService implements ApiService {
  async createShoppingList(name?: string): Promise<void> {
    await Promise.resolve();
    console.log(`Shopping list "${name ?? 'temporary'}" created.`);
  }

  async recordShoppingList(shoppingList: ShoppingListMessage): Promise<void> {
    await Promise.resolve();
    console.log(
      `Shopping list "${shoppingList.name}" recorded:`,
      shoppingList.products,
    );
  }

  getShoppingList(name?: string): Promise<ShoppingListMessage> {
    return Promise.resolve({
      name,
      products: [
        {
          checked: true,
          name: 'Naranjas',
          price: 2,
          quantity: '1 paquete',
        },
        {
          checked: true,
          name: 'Pan dulce',
          price: 1.5,
        },
        {
          checked: true,
          name: 'Leche entera',
          quantity: '1 litro',
        },
      ],
    });
  }

  getShoppingListSummaries(): Promise<ShoppingListSummaryMessage[]> {
    return Promise.resolve([{ name: undefined }, { name: 'Groceries' }]);
  }

  getRegisteredProducts(): Promise<ProductMessage[]> {
    return Promise.resolve([
      {
        checked: true,
        name: 'Manzanas',
        price: 2,
        quantity: '1 paquete',
      },
      {
        checked: true,
        name: 'Pan',
        price: 1.5,
        quantity: '2 barras',
      },
      {
        checked: true,
        name: 'Leche',
        quantity: '1 litro',
      },
    ]);
  }
}
