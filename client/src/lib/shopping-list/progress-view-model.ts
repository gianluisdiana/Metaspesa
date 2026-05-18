import { ProductMessage } from '@/lib/messages';

const PERCENTAGE_FACTOR = 100;

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
