import ApiService from '@/lib/api-service';
import { Product, ShoppingList } from '@/lib/domain';

export default class FakeApiService implements ApiService {
  async recordShoppingList(shoppingList: ShoppingList): Promise<void> {
    await Promise.resolve();
    console.log(
      `Shopping list "${shoppingList.name}" recorded:`,
      shoppingList.products,
    );
  }

  getCurrentShoppingList(): Promise<ShoppingList> {
    return Promise.resolve(
      new ShoppingList(
        [
          new Product('Naranjas', '1 paquete', 2),
          // eslint-disable-next-line @typescript-eslint/no-magic-numbers
          new Product('Pan dulce', undefined, 1.5, true),
          new Product('Leche entera', '1 litro'),
        ],
        undefined,
      ),
    );
  }

  getRegisteredProducts(): Promise<Product[]> {
    return Promise.resolve([
      new Product('Manzanas', '1 paquete', 2),
      // eslint-disable-next-line @typescript-eslint/no-magic-numbers
      new Product('Pan', '2 barras', 1.5),
      new Product('Leche', '1 litro'),
    ]);
  }
}
