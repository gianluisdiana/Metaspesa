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

function shoppingListsErrorResponse() {
  return NextResponse.json(
    { message: 'Could not load shopping lists.' },
    { status: 500 },
  );
}

export async function GET(request: NextRequest) {
  const token = await getAuthToken();
  const service = new GrpcApiService(token);
  const listsRequest = new ShoppingListsRequest(request);

  try {
    if (!listsRequest.hasListName) {
      return NextResponse.json(await service.getShoppingListSummaries());
    }

    return NextResponse.json(
      await service.getShoppingList(listsRequest.listName),
    );
  } catch {
    return shoppingListsErrorResponse();
  }
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
    if (new GrpcStatusError(error).alreadyExists) {
      try {
        return NextResponse.json({
          message:
            'Temporary list already exists. Name it or create a new one?',
          requiresTemporaryListName: true,
          shoppingList: await service.getShoppingList(),
          shoppingListSummaries: await service.getShoppingListSummaries(),
        });
      } catch {
        return shoppingListsErrorResponse();
      }
    }

    return shoppingListsErrorResponse();
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
    return shoppingListsErrorResponse();
  }
}
