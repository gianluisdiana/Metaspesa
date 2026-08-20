import { ProductMessage } from '@/lib/shopping-list-contracts';

import { ShoppingPriceViewModel } from './price-view-model';

export type ShoppingItemBadgeTone = 'item';

export type ShoppingItemBadgeViewModel = {
  label: string;
  tone: ShoppingItemBadgeTone;
};

export class CheckedShoppingItemViewModel {
  public constructor(private readonly product: ProductMessage) {}

  public get id(): string {
    return String(this.productFormatUid);
  }

  public get productFormatUid(): number {
    return this.product.productFormatUid;
  }

  public get name(): string {
    return this.product.name;
  }

  public get price(): string {
    return new ShoppingPriceViewModel(this.product.price).displayValue;
  }
}

export class UncheckedShoppingItemViewModel {
  public constructor(private readonly product: ProductMessage) {}

  public get badge(): ShoppingItemBadgeViewModel {
    return {
      label: 'Item',
      tone: 'item',
    };
  }

  public get categorySection(): string {
    return 'To buy';
  }

  public get id(): string {
    return String(this.productFormatUid);
  }

  public get productFormatUid(): number {
    return this.product.productFormatUid;
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
