import 'server-only';

import { ShoppingServiceClient } from '@/generated-protos/shopping/ShoppingService';
import * as grpc from '@grpc/grpc-js';

import ApiService from '@/lib/api-service';
import {
  ProductMessage,
  ShoppingItemUpdateMessage,
  ShoppingListMessage,
  ShoppingListSummaryMessage,
  ShoppingListUpdateMessage,
} from '@/lib/shopping-list-contracts';

import { GrpcClientFactory } from './grpc-client-factory';
import { logGrpcReadFailure } from './grpc-error-logger';
import { requireGrpcResponse } from './grpc-response-guards';
import { GrpcShoppingMapper } from './grpc-shopping-mapper';

const LOGGER_NAME = 'grpc-shopping-service';
const SERVICE_NAME = 'ShoppingService';

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

  async addItemsToList(
    shoppingListName: string | undefined,
    products: ProductMessage[],
  ): Promise<void> {
    for (const product of products) {
      if (!product.productFormatUid) {
        throw new Error('Product format UID is required to add an item.');
      }
    }

    await this.executeEmptyCall(resolve => {
      this.client.AddItemsToList(
        {
          ...(shoppingListName ? { shoppingListName } : {}),
          items: products.map(product => ({
            amount: 1,
            isChecked: product.checked,
            productFormatUid: product.productFormatUid,
          })),
        },
        this.metadata,
        resolve,
      );
    });
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
    } catch (error) {
      logGrpcReadFailure({
        error,
        grpcMethod: 'GetShoppingList',
        grpcService: SERVICE_NAME,
        loggerName: LOGGER_NAME,
      });
      throw error;
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
    } catch (error) {
      logGrpcReadFailure({
        error,
        grpcMethod: 'GetShoppingListSummaries',
        grpcService: SERVICE_NAME,
        loggerName: LOGGER_NAME,
      });
      throw error;
    }
  }

  async removeItem(
    shoppingListName: string | undefined,
    productFormatUid: number,
  ): Promise<void> {
    await this.executeEmptyCall(resolve => {
      this.client.RemoveItem(
        { productFormatUid, shoppingListName: shoppingListName ?? '' },
        this.metadata,
        resolve,
      );
    });
  }

  async recordShoppingList(shoppingListName?: string): Promise<void> {
    await this.executeEmptyCall(resolve => {
      this.client.RecordShoppingList(
        { shoppingListName: shoppingListName ?? '' },
        this.metadata,
        resolve,
      );
    });
  }

  async updateItem(
    shoppingListName: string | undefined,
    productFormatUid: number,
    update: ShoppingItemUpdateMessage,
  ): Promise<void> {
    await this.executeEmptyCall(resolve => {
      this.client.UpdateItem(
        {
          ...(update.amount === undefined ? {} : { amount: update.amount }),
          ...(update.checked === undefined
            ? {}
            : { isChecked: update.checked }),
          productFormatUid,
          shoppingListName: shoppingListName ?? '',
        },
        this.metadata,
        resolve,
      );
    });
  }

  async updateShoppingList(
    shoppingListName: string | undefined,
    update: ShoppingListUpdateMessage,
  ): Promise<void> {
    await this.executeEmptyCall(resolve => {
      this.client.UpdateShoppingList(
        {
          ...(update.name ? { listName: update.name } : {}),
          shoppingListName: shoppingListName ?? '',
        },
        this.metadata,
        resolve,
      );
    });
  }

  private async executeEmptyCall(
    call: (resolve: grpc.requestCallback<unknown>) => void,
  ): Promise<void> {
    await new Promise<void>((resolve, reject) => {
      call(err => {
        if (err) {
          reject(err);
          return;
        }

        resolve();
      });
    });
  }
}
