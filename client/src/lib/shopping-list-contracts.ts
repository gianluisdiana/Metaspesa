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

export interface ShoppingListSummaryMessage {
  name?: string;
}

export interface ShoppingItemUpdateMessage {
  checked?: boolean;
  name?: string;
  price?: number;
  quantity?: string;
}
