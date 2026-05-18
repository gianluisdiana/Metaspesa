import { MarketProductsResult, MarketSummaryMessage } from './market-contracts';

export interface MarketFilter {
  marketName?: string;
  brandNameSegment?: string;
  nameSegment?: string;
  page?: number;
  pageSize?: number;
}

export default interface MarketApiService {
  getMarketProducts(filter: MarketFilter): Promise<MarketProductsResult>;
  getMarkets(): Promise<MarketSummaryMessage[]>;
}
