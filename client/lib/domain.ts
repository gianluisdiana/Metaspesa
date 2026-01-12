export interface Product {
  name: string;
  quantity?: string;
  price?: number;
  checked: boolean;
}

export class ShoppingList {
  public constructor(public products: Product[]) {}

  public calculateTotal(): number {
    return this.products
      .filter(product => product.checked && product.price !== undefined)
      .reduce((total, product) => total + product.price!, 0);
  }

  public contains(product: Product): boolean {
    return this.products.some(p => p.name === product.name);
  }
}
