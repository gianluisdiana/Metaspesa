import { MarketProductsResult } from './market-messages';

export interface MarketFilter {
  marketName?: string;
  brandName?: string;
  nameSegment?: string;
}

export default interface MarketApiService {
  getMarketProducts(filter: MarketFilter): Promise<MarketProductsResult>;
  getMarkets(): Promise<string[]>;
}
