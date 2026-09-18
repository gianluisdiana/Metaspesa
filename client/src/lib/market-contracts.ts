import type {
  FormatResponse,
  MarketResponse,
  MoneyResponse,
  ProductPageResponse,
  ProductResponse,
  QuantityResponse,
} from '@/infrastructure/openapi_generated';

export type MarketSummaryMessage = MarketResponse;
export type MarketQuantityMessage = QuantityResponse;
export type MarketMoneyMessage = MoneyResponse;
export type MarketProductFormatMessage = FormatResponse;
export type MarketProductMessage = ProductResponse;
export type MarketProductsResult = ProductPageResponse;
