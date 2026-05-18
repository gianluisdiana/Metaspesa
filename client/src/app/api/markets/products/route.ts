import { cookies } from 'next/headers';
import { NextRequest, NextResponse } from 'next/server';

import GrpcMarketApiService from '@/infrastructure/grpc-market-api-service';

import { MarketProductsRequest } from './market-products-request';

export async function GET(request: NextRequest) {
  const cookieStore = await cookies();
  const token = cookieStore.get('auth_token')?.value ?? '';
  const filter = new MarketProductsRequest(
    request.nextUrl.searchParams,
  ).toFilter();
  const service = new GrpcMarketApiService(token);
  const result = await service.getMarketProducts(filter);

  return NextResponse.json(result);
}
