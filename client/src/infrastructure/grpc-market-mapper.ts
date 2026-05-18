import { MarketMessage, MarketSummaryMessage } from '@/lib/market-messages';

import { Market__Output } from '@/protos/markets/Market';
import { MarketSummary__Output } from '@/protos/markets/MarketSummary';

export class GrpcMarketMapper {
  public mapMarket(market: Market__Output): MarketMessage {
    return {
      name: market.name,
      products:
        market.products?.map(product => ({
          brandName: product.brandName,
          formats:
            product.formats?.map(format => ({
              imageUrl: format.imageUrl || null,
              price: Number(format.price),
              quantity: format.quantity,
            })) ?? [],
          name: product.name,
        })) ?? [],
    };
  }

  public mapMarkets(markets?: Market__Output[]): MarketMessage[] {
    return markets?.map(market => this.mapMarket(market)) ?? [];
  }

  public mapMarketSummaries(
    summaries?: MarketSummary__Output[],
  ): MarketSummaryMessage[] {
    return summaries?.map(summary => this.mapMarketSummary(summary)) ?? [];
  }

  public mapMarketSummary(
    summary: MarketSummary__Output,
  ): MarketSummaryMessage {
    return {
      logoUrl: summary.logoUrl || null,
      name: summary.name,
    };
  }
}
