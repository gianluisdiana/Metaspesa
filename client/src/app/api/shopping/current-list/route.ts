import { cookies } from 'next/headers';
import { NextResponse } from 'next/server';

import GrpcApiService from '@/infrastructure/grpc-api-service';

export async function GET() {
  const cookieStore = await cookies();
  const token = cookieStore.get('auth_token')?.value ?? '';
  const service = new GrpcApiService(token);

  return NextResponse.json(await service.getCurrentShoppingList());
}
