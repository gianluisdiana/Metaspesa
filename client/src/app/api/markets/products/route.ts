import { NextRequest, NextResponse } from 'next/server';

import GrpcMarketApiService from '@/infrastructure/grpc-market-api-service';
import { getAuthToken } from '@/lib/server/auth-cookie';

import { MarketProductsRequest } from './market-products-request';

export async function GET(request: NextRequest) {
  const token = await getAuthToken();
  const filter = new MarketProductsRequest(
    request.nextUrl.searchParams,
  ).toFilter();
  const service = new GrpcMarketApiService(token);
  const result = await service.getMarketProducts(filter);

  return NextResponse.json(result);
}
