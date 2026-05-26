import 'server-only';

import { MarketServiceClient } from '@/generated-protos/markets/MarketService';
import * as grpc from '@grpc/grpc-js';

import MarketApiService, { MarketFilter } from '@/lib/market-api-service';
import {
  MarketProductsResult,
  MarketSummaryMessage,
} from '@/lib/market-contracts';

import { GrpcClientFactory } from './grpc-client-factory';
import { logGrpcReadFailure } from './grpc-error-logger';
import { GrpcMarketMapper } from './grpc-market-mapper';

const LOGGER_NAME = 'grpc-market-service';
const SERVICE_NAME = 'MarketService';

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
    } catch (error) {
      logGrpcReadFailure({
        error,
        grpcMethod: 'GetMarketProducts',
        grpcService: SERVICE_NAME,
        loggerName: LOGGER_NAME,
      });
      throw error;
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
    } catch (error) {
      logGrpcReadFailure({
        error,
        grpcMethod: 'GetMarkets',
        grpcService: SERVICE_NAME,
        loggerName: LOGGER_NAME,
      });
      throw error;
    }
  }
}
