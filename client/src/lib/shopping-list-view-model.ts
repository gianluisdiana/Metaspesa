import {
  ProductMessage,
  ShoppingListMessage,
  ShoppingListSummaryMessage,
} from './messages';

const PERCENTAGE_FACTOR = 100;
const ROUND_FACTOR = 100;

export class ShoppingPriceViewModel {
  public constructor(private readonly price?: number) {}

  public get displayValue(): string {
    return this.price === undefined ? '-' : `$${this.roundedPrice.toFixed(2)}`;
  }

  private get roundedPrice(): number {
    return Math.round(this.price! * ROUND_FACTOR) / ROUND_FACTOR;
  }
}

export class CheckedShoppingItemViewModel {
  public constructor(
    private readonly product: ProductMessage,
    private readonly index: number,
  ) {}

  public get id(): string {
    return `${this.name}-${this.index}`;
  }

  public get name(): string {
    return this.product.name;
  }

  public get price(): string {
    return new ShoppingPriceViewModel(this.product.price).displayValue;
  }
}

export class UncheckedShoppingItemViewModel {
  public constructor(
    private readonly product: ProductMessage,
    private readonly index: number,
  ) {}

  public get badge(): { colorClass: string; label: string } {
    return {
      colorClass: 'bg-tertiary-container/30 text-on-tertiary-container',
      label: 'Item',
    };
  }

  public get categorySection(): string {
    return 'To buy';
  }

  public get id(): string {
    return `${this.name}-${this.index}`;
  }

  public get lowStock(): boolean {
    return false;
  }

  public get name(): string {
    return this.product.name;
  }

  public get price(): string {
    return new ShoppingPriceViewModel(this.product.price).displayValue;
  }

  public get qty(): string {
    return this.product.quantity ?? '-';
  }
}

export class ShoppingItemSectionViewModel {
  public constructor(
    public readonly label: string,
    public readonly items: UncheckedShoppingItemViewModel[],
  ) {}
}

export class ShoppingListTabViewModel {
  public constructor(
    private readonly summary: ShoppingListSummaryMessage,
    private readonly selectedListName?: string,
  ) {}

  public get active(): boolean {
    return this.name === this.selectedListName;
  }

  public get label(): string {
    return this.name && this.name.length > 0 ? this.name : 'Temporary List';
  }

  public get name(): string | undefined {
    return this.summary.name && this.summary.name.length > 0
      ? this.summary.name
      : undefined;
  }
}

export class ShoppingListTabsViewModel {
  public constructor(
    private readonly summaries: ShoppingListSummaryMessage[],
    private readonly selectedListName?: string,
    private readonly fallbackList?: ShoppingListMessage,
  ) {}

  public get tabs(): ShoppingListTabViewModel[] {
    return this.normalizedSummaries
      .sort((a, b) => {
        // eslint-disable-next-line @typescript-eslint/no-magic-numbers
        if (!a.name) return -1;
        if (!b.name) return 1;
        return a.name.localeCompare(b.name);
      })
      .map(
        summary => new ShoppingListTabViewModel(summary, this.selectedListName),
      );
  }

  private get normalizedSummaries(): ShoppingListSummaryMessage[] {
    if (this.summaries.length > 0) {
      return this.summaries;
    }

    return [{ name: this.fallbackList?.name }];
  }
}

export class ShoppingProgressViewModel {
  public constructor(private readonly products: ProductMessage[]) {}

  public get checkedCount(): number {
    return this.products.filter(product => product.checked).length;
  }

  public get label(): string {
    return `${this.checkedCount} of ${this.totalCount} items found`;
  }

  public get percentage(): number {
    return this.totalCount === 0
      ? 0
      : (this.checkedCount / this.totalCount) * PERCENTAGE_FACTOR;
  }

  public get totalCount(): number {
    return this.products.length;
  }
}

export class ShoppingListViewModel {
  public constructor(private readonly shoppingList: ShoppingListMessage) {}

  public get checkedItems(): CheckedShoppingItemViewModel[] {
    return this.checkedProducts.map(
      (product, index) => new CheckedShoppingItemViewModel(product, index),
    );
  }

  public get checkedTotal(): string {
    return new ShoppingPriceViewModel(this.sumPrices(this.checkedProducts))
      .displayValue;
  }

  public get estimatedTotal(): string {
    return new ShoppingPriceViewModel(this.sumPrices(this.products))
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

  private sumPrices(products: ProductMessage[]): number {
    const total = products.reduce(
      (sum, product) => sum + (product.price ?? 0),
      0,
    );
    return Math.round(total * ROUND_FACTOR) / ROUND_FACTOR;
  }
}
