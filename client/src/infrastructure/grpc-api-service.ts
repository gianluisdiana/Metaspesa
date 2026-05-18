import 'server-only';

import path from 'node:path';

import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';

import ApiService from '@/lib/api-service';
import {
  ProductMessage,
  ShoppingListMessage,
  ShoppingListSummaryMessage,
} from '@/lib/messages';

import { ShoppingList__Output } from '@/protos/shopping/ShoppingList';
import { ShoppingListSummary__Output } from '@/protos/shopping/ShoppingListSummary';
import { ShoppingServiceClient } from '@/protos/shopping/ShoppingService';
import { ProtoGrpcType } from '@/protos/shopping_service';

import { createTracingMetadata } from './grpc-metadata';

export default class GrpcApiService implements ApiService {
  private readonly client: ShoppingServiceClient;
  private readonly metadata: grpc.Metadata;

  constructor(token: string) {
    const protoPath = path.resolve(
      process.cwd(),
      'src/infrastructure/protos/Shopping/shopping_service.proto',
    );
    const packageDefinition = protoLoader.loadSync(protoPath, {
      defaults: true,
    });
    const { ShoppingService } = (
      grpc.loadPackageDefinition(packageDefinition) as unknown as ProtoGrpcType
    ).Metaspesa.Protos.Shopping;

    const credentials =
      process.env.BACKEND_SECURE === 'true'
        ? grpc.credentials.createSsl()
        : grpc.credentials.createInsecure();
    this.client = new ShoppingService(
      process.env.GRPC_SERVER_URL as string,
      credentials,
    );
    this.metadata = createTracingMetadata();
    this.metadata.set('Authorization', `Bearer ${token}`);
  }

  async createShoppingList(name?: string): Promise<void> {
    await new Promise<void>((resolve, reject) => {
      this.client.CreateShoppingList(
        name ? { name } : {},
        this.metadata,
        err => {
          if (err) {
            reject(err);
            return;
          }

          resolve();
        },
      );
    });
  }

  async getShoppingList(name?: string): Promise<ShoppingListMessage> {
    try {
      const shoppingList = await new Promise<ShoppingListMessage>(
        (resolve, reject) => {
          this.client.GetShoppingList(
            name ? { shoppingListName: name } : {},
            this.metadata,
            (err, response) => {
              if (err) {
                reject(err);
                return;
              }

              resolve(this.mapShoppingList(response!.shoppingList!));
            },
          );
        },
      );

      return shoppingList;
    } catch {
      return {
        name: '',
        products: [],
      };
    }
  }

  async getShoppingListSummaries(): Promise<ShoppingListSummaryMessage[]> {
    try {
      return await new Promise<ShoppingListSummaryMessage[]>(
        (resolve, reject) => {
          this.client.GetShoppingListSummaries(
            {},
            this.metadata,
            (err, response) => {
              if (err) {
                reject(err);
                return;
              }

              resolve(
                response!.shoppingLists?.map(summary =>
                  this.mapShoppingListSummary(summary),
                ) ?? [],
              );
            },
          );
        },
      );
    } catch {
      return [];
    }
  }

  async getRegisteredProducts(): Promise<ProductMessage[]> {
    try {
      return await new Promise<ProductMessage[]>((resolve, reject) => {
        this.client.GetRegisteredItems({}, this.metadata, (err, response) => {
          if (err) {
            reject(err);
            return;
          }

          resolve(
            response!.items?.map(item => ({
              checked: item.checked ?? false,
              name: item.name,
              price: Number(item.price),
              quantity: item.quantity,
            })) ?? [],
          );
        });
      });
    } catch {
      return [];
    }
  }

  recordShoppingList(shoppingList: ShoppingListMessage): Promise<void> {
    throw new Error(
      `Method not implemented. Received: ${JSON.stringify(shoppingList)}`,
    );
  }

  private mapShoppingList(proto: ShoppingList__Output): ShoppingListMessage {
    const factor = 100;
    const products: ProductMessage[] =
      proto.items?.map(itemProto => ({
        checked: itemProto.checked ?? false,
        name: itemProto.name,
        price:
          itemProto.price === undefined
            ? undefined
            : Math.round(Number(itemProto.price) * factor) / factor,
        quantity: itemProto.quantity,
      })) ?? [];
    return {
      name: proto.name,
      products,
    };
  }

  private mapShoppingListSummary(
    proto: ShoppingListSummary__Output,
  ): ShoppingListSummaryMessage {
    return { name: proto.name };
  }
}
