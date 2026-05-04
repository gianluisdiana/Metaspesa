import { MarketProductsResult, MarketSummaryMessage } from './market-messages';

export interface MarketFilter {
  marketName?: string;
  brandNameSegment?: string;
  nameSegment?: string;
}

export default interface MarketApiService {
  getMarketProducts(filter: MarketFilter): Promise<MarketProductsResult>;
  getMarkets(): Promise<MarketSummaryMessage[]>;
}
