export interface MarketSummaryMessage {
  name: string;
  logoUrl: string | null;
}

export interface MarketProductFormatMessage {
  quantity: string;
  price: number;
  imageUrl: string | null;
}

export interface MarketProductMessage {
  name: string;
  brandName: string;
  formats: MarketProductFormatMessage[];
}

export interface MarketMessage {
  name: string;
  products: MarketProductMessage[];
}

export interface MarketProductsResult {
  markets: MarketMessage[];
  totalProducts: number;
}
