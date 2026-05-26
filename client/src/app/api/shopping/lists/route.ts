import { NextRequest, NextResponse } from 'next/server';

import GrpcApiService from '@/infrastructure/grpc-api-service';
import { getAuthToken } from '@/lib/server/auth-cookie';

import {
  GrpcStatusError,
  ShoppingListsRequest,
} from './shopping-lists-request';

type UpdateShoppingListBody = {
  shoppingListName?: string;
  update?: {
    name?: string;
  };
};

export async function GET(request: NextRequest) {
  const token = await getAuthToken();
  const service = new GrpcApiService(token);
  const listsRequest = new ShoppingListsRequest(request);

  if (!listsRequest.hasListName) {
    return NextResponse.json(await service.getShoppingListSummaries());
  }

  return NextResponse.json(
    await service.getShoppingList(listsRequest.listName),
  );
}

export async function POST() {
  const token = await getAuthToken();
  const service = new GrpcApiService(token);

  try {
    await service.createShoppingList();
    return NextResponse.json({
      message: 'Temporary list created.',
      shoppingList: await service.getShoppingList(),
      shoppingListSummaries: await service.getShoppingListSummaries(),
    });
  } catch (error) {
    const shoppingList = await service.getShoppingList();
    const shoppingListSummaries = await service.getShoppingListSummaries();
    if (new GrpcStatusError(error).alreadyExists) {
      return NextResponse.json({
        message: 'Temporary list already exists. Name it or create a new one?',
        requiresTemporaryListName: true,
        shoppingList,
        shoppingListSummaries,
      });
    }

    return NextResponse.json(
      {
        message: 'Could not create a temporary list.',
        shoppingList,
        shoppingListSummaries,
      },
      { status: 500 },
    );
  }
}

export async function PATCH(request: NextRequest) {
  const token = await getAuthToken();
  const service = new GrpcApiService(token);
  const body = (await request.json()) as UpdateShoppingListBody;
  const name = body.update?.name?.trim();

  if (!name) {
    return NextResponse.json(
      { message: 'Shopping list name is required.' },
      { status: 400 },
    );
  }

  try {
    await service.updateShoppingList(body.shoppingListName, { name });
    await service.createShoppingList();

    return NextResponse.json({
      message: 'Temporary list created.',
      shoppingList: await service.getShoppingList(),
      shoppingListSummaries: await service.getShoppingListSummaries(),
    });
  } catch {
    return NextResponse.json(
      {
        message: 'Could not create a temporary list.',
        shoppingList: await service.getShoppingList(body.shoppingListName),
        shoppingListSummaries: await service.getShoppingListSummaries(),
      },
      { status: 500 },
    );
  }
}
