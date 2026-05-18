import 'server-only';

import * as grpc from '@grpc/grpc-js';

import ApiService from '@/lib/api-service';
import {
  ProductMessage,
  ShoppingListMessage,
  ShoppingListSummaryMessage,
} from '@/lib/messages';

import { ShoppingList__Output } from '@/protos/shopping/ShoppingList';
import { ShoppingListSummary__Output } from '@/protos/shopping/ShoppingListSummary';
import { ShoppingServiceClient } from '@/protos/shopping/ShoppingService';

import { GrpcClientFactory } from './grpc-client-factory';

export default class GrpcApiService implements ApiService {
  private readonly client: ShoppingServiceClient;
  private readonly metadata: grpc.Metadata;

  constructor(token: string, factory = new GrpcClientFactory()) {
    this.client = factory.createShoppingServiceClient();
    this.metadata = factory.createAuthorizedMetadata(token);
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
