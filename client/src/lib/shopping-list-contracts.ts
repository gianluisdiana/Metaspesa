export interface ProductMessage {
  productFormatUid: number;
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
  amount?: number;
  checked?: boolean;
}

export interface ShoppingListUpdateMessage {
  name?: string;
}
