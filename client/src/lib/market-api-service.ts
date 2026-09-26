import { MarketProductsResult, MarketSummaryMessage } from './market-contracts';

export type MarketFilter = {
  brand?: string;
  marketId?: string[];
  page?: number;
  pageSize?: number;
  query?: string;
  sort?: 'name' | 'priceAsc' | 'priceDesc';
};

export default interface MarketApiService {
  getMarketProducts(filter: MarketFilter): Promise<MarketProductsResult>;
  getMarkets(): Promise<MarketSummaryMessage[]>;
}
