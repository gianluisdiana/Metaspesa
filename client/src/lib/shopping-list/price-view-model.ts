import { dollars } from '@/lib/formatters/money-formatter';

export class ShoppingPriceViewModel {
  public constructor(private readonly price?: number) {}

  public get displayValue(): string {
    return dollars.format(this.price);
  }
}

export function sumShoppingPrices(
  products: ReadonlyArray<{ price?: number }>,
): number {
  return dollars.sum(products.map(product => product.price));
}
