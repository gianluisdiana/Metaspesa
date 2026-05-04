import 'server-only';

import path from 'node:path';

import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';

import MarketApiService, { MarketFilter } from '@/lib/market-api-service';
import {
  MarketMessage,
  MarketProductsResult,
  MarketSummaryMessage,
} from '@/lib/market-messages';

import { Market__Output } from '@/protos/markets/Market';
import { MarketServiceClient } from '@/protos/markets/MarketService';
import { MarketSummary__Output } from '@/protos/markets/MarketSummary';
import { ProtoGrpcType } from '@/protos/markets_service';

import { createTracingMetadata } from './grpc-metadata';

export default class GrpcMarketApiService implements MarketApiService {
  private readonly client: MarketServiceClient;
  private readonly metadata: grpc.Metadata;

  constructor(token: string) {
    const protoPath = path.resolve(
      process.cwd(),
      'src/infrastructure/protos/Markets/markets_service.proto',
    );
    const packageDefinition = protoLoader.loadSync(protoPath);
    const { MarketService } = (
      grpc.loadPackageDefinition(packageDefinition) as unknown as ProtoGrpcType
    ).Metaspesa.Protos.Markets;

    const credentials =
      process.env.BACKEND_SECURE === 'true'
        ? grpc.credentials.createSsl()
        : grpc.credentials.createInsecure();
    this.client = new MarketService(
      process.env.GRPC_SERVER_URL as string,
      credentials,
    );
    this.metadata = createTracingMetadata();
    this.metadata.set('Authorization', `Bearer ${token}`);
  }

  async getMarketProducts(filter: MarketFilter): Promise<MarketProductsResult> {
    try {
      return await new Promise<MarketProductsResult>((resolve, reject) => {
        this.client.GetMarketProducts(
          {
            brandName: filter.brandName,
            marketName: filter.marketName,
            nameSegment: filter.nameSegment,
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
            price: f.price,
            quantity: f.quantity,
          })) ?? [],
        name: p.name,
      })) ?? [],
  };
}
