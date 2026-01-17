export class Product {
  public constructor(
    public name: string,
    public quantity: string | undefined = undefined,
    public price: number | undefined = undefined,
    public checked: boolean = false,
  ) {}

  public hasValidQuantity(): boolean {
    const maximumQuantityLength = 50;
    return (
      this.quantity === undefined ||
      this.quantity.length <= maximumQuantityLength
    );
  }

  public hasValidPrice(): boolean {
    return this.price === undefined || this.price >= 0;
  }
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
