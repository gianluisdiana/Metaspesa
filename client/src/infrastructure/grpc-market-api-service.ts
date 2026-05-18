import 'server-only';

import * as grpc from '@grpc/grpc-js';

import MarketApiService, { MarketFilter } from '@/lib/market-api-service';
import {
  MarketProductsResult,
  MarketSummaryMessage,
} from '@/lib/market-messages';

import { MarketServiceClient } from '@/protos/markets/MarketService';

import { GrpcClientFactory } from './grpc-client-factory';
import { GrpcMarketMapper } from './grpc-market-mapper';

export default class GrpcMarketApiService implements MarketApiService {
  private readonly client: MarketServiceClient;
  private readonly mapper: GrpcMarketMapper;
  private readonly metadata: grpc.Metadata;

  constructor(
    token: string,
    factory = new GrpcClientFactory(),
    mapper = new GrpcMarketMapper(),
  ) {
    this.client = factory.createMarketServiceClient();
    this.mapper = mapper;
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
              markets: this.mapper.mapMarkets(response?.markets),
              totalProducts: response?.totalProducts ?? 0,
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
          resolve(this.mapper.mapMarketSummaries(response?.markets));
        });
      });
    } catch {
      return [];
    }
  }
}
