import 'server-only';

import * as grpc from '@grpc/grpc-js';

import MarketApiService, { MarketFilter } from '@/lib/market-api-service';
import {
  MarketMessage,
  MarketProductsResult,
  MarketSummaryMessage,
} from '@/lib/market-messages';

import { Market__Output } from '@/protos/markets/Market';
import { MarketServiceClient } from '@/protos/markets/MarketService';
import { MarketSummary__Output } from '@/protos/markets/MarketSummary';

import { GrpcClientFactory } from './grpc-client-factory';

export default class GrpcMarketApiService implements MarketApiService {
  private readonly client: MarketServiceClient;
  private readonly metadata: grpc.Metadata;

  constructor(token: string, factory = new GrpcClientFactory()) {
    this.client = factory.createMarketServiceClient();
    this.metadata = factory.createAuthorizedMetadata(token);
  }

  async getMarketProducts(filter: MarketFilter): Promise<MarketProductsResult> {
    try {
      return await new Promise<MarketProductsResult>((resolve, reject) => {
        this.client.GetMarketProducts(
          {
            brandNameSegment: filter.brandNameSegment,
            marketName: filter.marketName,
            nameSegment: filter.nameSegment,
            page: filter.page,
            pageSize: filter.pageSize,
          },
          this.metadata,
          (err, response) => {
            if (err) {
              reject(err);
              return;
            }
            resolve({
              markets: response!.markets?.map(mapMarket) ?? [],
              totalProducts: response!.totalProducts ?? 0,
            });
          },
        );
      });
    } catch {
      return { markets: [], totalProducts: 0 };
    }
  }

  async getMarkets(): Promise<MarketSummaryMessage[]> {
    try {
      return await new Promise<MarketSummaryMessage[]>((resolve, reject) => {
        this.client.GetMarkets({}, this.metadata, (err, response) => {
          if (err) {
            reject(err);
            return;
          }
          resolve(response!.markets?.map(mapMarketSummary) ?? []);
        });
      });
    } catch {
      return [];
    }
  }
}

function mapMarketSummary(
  summary: MarketSummary__Output,
): MarketSummaryMessage {
  return {
    logoUrl: summary.logoUrl || null,
    name: summary.name,
  };
}

function mapMarket(market: Market__Output): MarketMessage {
  return {
    name: market.name,
    products:
      market.products?.map(p => ({
        brandName: p.brandName,
        formats:
          p.formats?.map(f => ({
            imageUrl: f.imageUrl || null,
            price: Number(f.price),
            quantity: f.quantity,
          })) ?? [],
        name: p.name,
      })) ?? [],
  };
}
