import ApiService from '@/lib/api-service';
import {
  ProductMessage,
  ShoppingItemUpdateMessage,
  ShoppingListMessage,
  ShoppingListSummaryMessage,
  ShoppingListUpdateMessage,
} from '@/lib/shopping-list-contracts';

export default class FakeApiService implements ApiService {
  async addItemsToList(
    shoppingListName: string | undefined,
    products: ProductMessage[],
  ): Promise<void> {
    const list = await this.getShoppingList(shoppingListName);
    list.products.push(...products);
  }

  async createShoppingList(name?: string): Promise<void> {
    await Promise.resolve();
    console.log(`Shopping list "${name ?? 'temporary'}" created.`);
  }

  async recordShoppingList(shoppingListName?: string): Promise<void> {
    await Promise.resolve();
    console.log(`Shopping list "${shoppingListName ?? 'temporary'}" recorded.`);
  }

  getShoppingList(name?: string): Promise<ShoppingListMessage> {
    return Promise.resolve({
      name,
      products: [
        {
          checked: true,
          name: 'Naranjas',
          price: 2,
          productFormatUid: 1,
          quantity: '1 paquete',
        },
        {
          checked: true,
          name: 'Pan dulce',
          price: 1.5,
          productFormatUid: 2,
        },
        {
          checked: true,
          name: 'Leche entera',
          productFormatUid: 3,
          quantity: '1 litro',
        },
      ],
    });
  }

  getShoppingListSummaries(): Promise<ShoppingListSummaryMessage[]> {
    return Promise.resolve([{ name: undefined }, { name: 'Groceries' }]);
  }

  async removeItem(
    shoppingListName: string | undefined,
    productFormatUid: number,
  ): Promise<void> {
    const list = await this.getShoppingList(shoppingListName);
    list.products = list.products.filter(
      product => product.productFormatUid !== productFormatUid,
    );
  }

  async updateItem(
    shoppingListName: string | undefined,
    productFormatUid: number,
    update: ShoppingItemUpdateMessage,
  ): Promise<void> {
    const list = await this.getShoppingList(shoppingListName);
    list.products = list.products.map(product =>
      product.productFormatUid === productFormatUid
        ? { ...product, ...update }
        : product,
    );
  }

  async updateShoppingList(
    shoppingListName: string | undefined,
    update: ShoppingListUpdateMessage,
  ): Promise<void> {
    await Promise.resolve();
    console.log(
      `Shopping list "${shoppingListName ?? 'temporary'}" updated:`,
      update,
    );
  }
}
