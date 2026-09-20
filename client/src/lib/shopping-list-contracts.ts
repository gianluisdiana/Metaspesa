export interface ProductMessage {
  productFormatUid: number;
  name: string;
  quantity?: string;
  price?: number;
  checked: boolean;
}

export interface ShoppingListMessage {
  id?: number;
  products: ProductMessage[];
  name?: string;
}

export interface ShoppingListSummaryMessage {
  id?: number;
  name?: string;
  isTemporary?: boolean;
}

export interface ShoppingItemUpdateMessage {
  amount?: number;
  checked?: boolean;
}

export interface ShoppingListUpdateMessage {
  name?: string;
}
