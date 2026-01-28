import 'server-only';

import path from 'node:path';

import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';

import ApiService from '@/lib/api-service';
import { ProductMessage, ShoppingListMessage } from '@/lib/messages';

import { ShoppingList__Output } from '@/protos/shopping/ShoppingList';
import { ShoppingServiceClient } from '@/protos/shopping/ShoppingService';
import { ProtoGrpcType } from '@/protos/shopping_service';

export default class GrpcApiService implements ApiService {
  private readonly client: ShoppingServiceClient;

  constructor() {
    const protoPath = path.resolve(
      process.cwd(),
      'infrastructure/protos/Shopping/shopping_service.proto',
    );
    const packageDefinition = protoLoader.loadSync(protoPath);
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
  }

  getCurrentShoppingList(): Promise<ShoppingListMessage> {
    return new Promise((resolve, reject) => {
      this.client.GetCurrentShoppingList({}, (err, response) => {
        if (err) {
          reject(err);
          return;
        }

        resolve(this.mapShoppingList(response!.shoppingList!));
      });
    });
  }

  getRegisteredProducts(): Promise<ProductMessage[]> {
    throw new Error('Method not implemented.');
  }

  recordShoppingList(shoppingList: ShoppingListMessage): Promise<void> {
    throw new Error('Method not implemented.');
  }

  private mapShoppingList(proto: ShoppingList__Output): ShoppingListMessage {
    const products: ProductMessage[] = proto.products.map(productProto => ({
      checked: productProto.checked,
      name: productProto.name,
      price: productProto.price,
      quantity: productProto.quantity,
    }));
    return {
      name: proto.name,
      products,
    };
  }
}
