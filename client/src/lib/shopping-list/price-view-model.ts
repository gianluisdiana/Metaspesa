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

export function sumShoppingPrices(
  products: ReadonlyArray<{ price?: number }>,
): number {
  const total = products.reduce((sum, product) => sum + (product.price ?? 0), 0);
  return Math.round(total * ROUND_FACTOR) / ROUND_FACTOR;
}
