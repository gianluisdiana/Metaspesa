export interface ProductMessage {
  name: string;
  quantity?: string;
  price?: number;
  checked: boolean;
}

export interface ShoppingListMessage {
  products: ProductMessage[];
  name?: string;
}
