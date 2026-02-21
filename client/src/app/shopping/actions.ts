'use server';

import GrpcApiService from '@/infrastructure/grpc-api-service';
import { ShoppingListMessage } from '@/lib/messages';

export async function recordShoppingList(shoppingList: ShoppingListMessage) {
  const api = new GrpcApiService();
  await api.recordShoppingList(shoppingList);
}
