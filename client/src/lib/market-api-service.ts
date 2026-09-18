import type { GetProductsData } from '@/infrastructure/openapi_generated';

import { MarketProductsResult, MarketSummaryMessage } from './market-contracts';

export type MarketFilter = NonNullable<GetProductsData['query']>;

export default interface MarketApiService {
  getMarketProducts(filter: MarketFilter): Promise<MarketProductsResult>;
  getMarkets(): Promise<MarketSummaryMessage[]>;
}
