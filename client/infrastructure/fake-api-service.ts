import ApiService from '@/lib/api-service';
import { ShoppingList, Product } from '@/lib/domain';

export default class FakeApiService implements ApiService {
  getCurrentShoppingList(): Promise<ShoppingList> {
    return Promise.resolve(
      new ShoppingList([
        new Product('Naranjas', '1 paquete', 2),
        // eslint-disable-next-line @typescript-eslint/no-magic-numbers
        new Product('Pan dulce', undefined, 1.5, true),
        new Product('Leche entera', '1 litro'),
      ]),
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
