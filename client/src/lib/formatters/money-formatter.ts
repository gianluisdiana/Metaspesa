export type CurrencySymbol = '$' | '€';

const DECIMAL_PLACES = 2;
const MISSING_PRICE_LABEL = '-';
const ROUND_FACTOR = 100;

export class MoneyFormatter {
  public constructor(private readonly currencySymbol: CurrencySymbol) {}

  public format(price?: number): string {
    return price === undefined
      ? MISSING_PRICE_LABEL
      : `${this.currencySymbol}${this.round(price).toFixed(DECIMAL_PLACES)}`;
  }

  public round(price: number): number {
    return Math.round(price * ROUND_FACTOR) / ROUND_FACTOR;
  }

  public sum(prices: ReadonlyArray<number | undefined>): number {
    return this.round(prices.reduce((total, price) => total + (price ?? 0), 0));
  }
}

export const dollars = new MoneyFormatter('$');
export const euros = new MoneyFormatter('€');
