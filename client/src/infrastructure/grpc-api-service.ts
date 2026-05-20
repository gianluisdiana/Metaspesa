import 'server-only';

import { ShoppingServiceClient } from '@/generated-protos/shopping/ShoppingService';
import * as grpc from '@grpc/grpc-js';

import ApiService from '@/lib/api-service';
import {
  ProductMessage,
  ShoppingListMessage,
  ShoppingListSummaryMessage,
} from '@/lib/shopping-list-contracts';

import { GrpcClientFactory } from './grpc-client-factory';
import { requireGrpcResponse } from './grpc-response-guards';
import { GrpcShoppingMapper } from './grpc-shopping-mapper';

export default class GrpcApiService implements ApiService {
  private readonly client: ShoppingServiceClient;
  private readonly mapper: GrpcShoppingMapper;
  private readonly metadata: grpc.Metadata;

  constructor(
    token: string,
    factory = new GrpcClientFactory(),
    mapper = new GrpcShoppingMapper(),
  ) {
    this.client = factory.createShoppingServiceClient();
    this.mapper = mapper;
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

              const responseMessage = requireGrpcResponse(
                response,
                'GetShoppingListResponse',
              );
              resolve(
                this.mapper.mapShoppingList(responseMessage.shoppingList),
              );
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
                this.mapper.mapShoppingListSummaries(response?.shoppingLists),
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
            this.mapper.mapRegisteredItems(
              requireGrpcResponse(response, 'RegisteredItemsResponse'),
            ),
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
}
