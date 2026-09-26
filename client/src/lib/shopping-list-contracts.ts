export interface ProductMessage {
  productFormatUid: string;
  name: string;
  quantity?: string;
  price?: number;
  checked: boolean;
}

export interface ShoppingListMessage {
  id?: string;
  products: ProductMessage[];
  name?: string;
}

export interface ShoppingListSummaryMessage {
  id?: string;
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
