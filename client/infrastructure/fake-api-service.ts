import ApiService from '@/lib/api-service';
import { ShoppingList, Product } from '@/lib/domain';

export default class FakeApiService implements ApiService {
  getCurrentShoppingList(): Promise<ShoppingList> {
    return Promise.resolve(
      new ShoppingList([
        { checked: false, name: 'Naranjas', price: 2, quantity: '1 paquete' },
        { checked: true, name: 'Pan dulce', price: 1.5 },
        { checked: false, name: 'Leche entera', quantity: '1 litro' },
      ]),
    );
  }

  getRegisteredProducts(): Promise<Product[]> {
    return Promise.resolve([
      { checked: false, name: 'Manzanas', price: 2, quantity: '1 paquete' },
      { checked: false, name: 'Pan', price: 1.5, quantity: '2 barras' },
      { checked: false, name: 'Leche', quantity: '1 litro' },
    ]);
  }
}
