import { cookies } from 'next/headers';
import { NextRequest, NextResponse } from 'next/server';

import GrpcApiService from '@/infrastructure/grpc-api-service';

import {
  GrpcStatusError,
  ShoppingListsRequest,
} from './shopping-lists-request';

export async function GET(request: NextRequest) {
  const cookieStore = await cookies();
  const token = cookieStore.get('auth_token')?.value ?? '';
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
  const cookieStore = await cookies();
  const token = cookieStore.get('auth_token')?.value ?? '';
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
        message: 'Temporary list already exists.',
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
