import { cookies } from 'next/headers';
import { NextRequest, NextResponse } from 'next/server';

import GrpcApiService from '@/infrastructure/grpc-api-service';

const ALREADY_EXISTS = 6;

function getErrorCode(error: unknown): number | undefined {
  if (typeof error !== 'object' || error === null || !('code' in error)) {
    return undefined;
  }

  return Number(error.code);
}

function getListName(request: NextRequest): string | undefined {
  const name = request.nextUrl.searchParams.get('name');
  return name && name.length > 0 ? name : undefined;
}

export async function GET(request: NextRequest) {
  const cookieStore = await cookies();
  const token = cookieStore.get('auth_token')?.value ?? '';
  const service = new GrpcApiService(token);

  return NextResponse.json(await service.getShoppingList(getListName(request)));
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
    });
  } catch (error) {
    const shoppingList = await service.getShoppingList();
    if (getErrorCode(error) === ALREADY_EXISTS) {
      return NextResponse.json({
        message: 'Temporary list already exists.',
        shoppingList,
      });
    }

    return NextResponse.json(
      {
        message: 'Could not create a temporary list.',
        shoppingList,
      },
      { status: 500 },
    );
  }
}
