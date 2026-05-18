import {
  ProductMessage,
  ShoppingListMessage,
} from '@/lib/shopping-list-contracts';

import {
  CheckedShoppingItemViewModel,
  ShoppingItemSectionViewModel,
  UncheckedShoppingItemViewModel,
} from './item-view-model';
import { ShoppingPriceViewModel, sumShoppingPrices } from './price-view-model';
import { ShoppingProgressViewModel } from './progress-view-model';

export class ShoppingListViewModel {
  public constructor(private readonly shoppingList: ShoppingListMessage) {}

  public get checkedItems(): CheckedShoppingItemViewModel[] {
    return this.checkedProducts.map(
      (product, index) => new CheckedShoppingItemViewModel(product, index),
    );
  }

  public get checkedTotal(): string {
    return new ShoppingPriceViewModel(sumShoppingPrices(this.checkedProducts))
      .displayValue;
  }

  public get estimatedTotal(): string {
    return new ShoppingPriceViewModel(sumShoppingPrices(this.products))
      .displayValue;
  }

  public get hasItems(): boolean {
    return this.itemCount > 0;
  }

  public get itemCount(): number {
    return this.products.length;
  }

  public get itemCountLabel(): string {
    return this.itemCount === 1 ? '1 item' : `${this.itemCount} items`;
  }

  public get listName(): string {
    return this.shoppingList.name && this.shoppingList.name.length > 0
      ? this.shoppingList.name
      : 'Temporary List';
  }

  public get progress(): ShoppingProgressViewModel {
    return new ShoppingProgressViewModel(this.products);
  }

  public get uncheckedSections(): ShoppingItemSectionViewModel[] {
    const uncheckedItems = this.products
      .filter(product => !product.checked)
      .map(
        (product, index) => new UncheckedShoppingItemViewModel(product, index),
      );
    const sectionNames = [
      ...new Set(uncheckedItems.map(item => item.categorySection)),
    ];

    return sectionNames.map(
      sectionName =>
        new ShoppingItemSectionViewModel(
          sectionName,
          uncheckedItems.filter(item => item.categorySection === sectionName),
        ),
    );
  }

  private get checkedProducts(): ProductMessage[] {
    return this.products.filter(product => product.checked);
  }

  private get products(): ProductMessage[] {
    return this.shoppingList.products;
  }
}
