import { NextRequest, NextResponse } from 'next/server';

import GrpcApiService from '@/infrastructure/grpc-api-service';
import { getAuthToken } from '@/lib/server/auth-cookie';
import {
  ProductMessage,
  ShoppingItemUpdateMessage,
} from '@/lib/shopping-list-contracts';

type AddItemsBody = {
  items?: ProductMessage[];
  shoppingListName?: string;
};

type UpdateItemBody = {
  itemName?: string;
  shoppingListName?: string;
  update?: ShoppingItemUpdateMessage;
};

type RemoveItemBody = {
  itemName?: string;
  shoppingListName?: string;
};

async function responseForList(service: GrpcApiService, listName?: string) {
  return NextResponse.json({
    shoppingList: await service.getShoppingList(listName),
    shoppingListSummaries: await service.getShoppingListSummaries(),
  });
}

export async function POST(request: NextRequest) {
  const token = await getAuthToken();
  const service = new GrpcApiService(token);
  const body = (await request.json()) as AddItemsBody;
  const items = body.items ?? [];

  if (items.length === 0) {
    return NextResponse.json({ message: 'No items to add.' }, { status: 400 });
  }

  await service.addItemsToList(body.shoppingListName, items);
  return await responseForList(service, body.shoppingListName);
}

export async function PATCH(request: NextRequest) {
  const token = await getAuthToken();
  const service = new GrpcApiService(token);
  const body = (await request.json()) as UpdateItemBody;

  if (!body.itemName || !body.update) {
    return NextResponse.json(
      { message: 'Item name and update are required.' },
      { status: 400 },
    );
  }

  await service.updateItem(body.shoppingListName, body.itemName, body.update);
  return await responseForList(service, body.shoppingListName);
}

export async function DELETE(request: NextRequest) {
  const token = await getAuthToken();
  const service = new GrpcApiService(token);
  const body = (await request.json()) as RemoveItemBody;

  if (!body.itemName) {
    return NextResponse.json(
      { message: 'Item name is required.' },
      { status: 400 },
    );
  }

  await service.removeItem(body.shoppingListName, body.itemName);
  return await responseForList(service, body.shoppingListName);
}
