import { cookies } from 'next/headers';
import { NextRequest, NextResponse } from 'next/server';

import GrpcMarketApiService from '@/infrastructure/grpc-market-api-service';
import { MarketFilter } from '@/lib/market-api-service';

const DEFAULT_PAGE_SIZE = 20;

function stringParam(params: URLSearchParams, key: string): string | undefined {
  const value = params.get(key);
  return value && value.length > 0 ? value : undefined;
}

function positiveNumberParam(
  params: URLSearchParams,
  key: string,
  fallback: number,
): number {
  const value = Number(params.get(key));
  return Number.isInteger(value) && value > 0 ? value : fallback;
}

export async function GET(request: NextRequest) {
  const cookieStore = await cookies();
  const token = cookieStore.get('auth_token')?.value ?? '';
  const params = request.nextUrl.searchParams;
  const filter: MarketFilter = {
    brandNameSegment: stringParam(params, 'brand_name'),
    marketName: stringParam(params, 'market_name'),
    nameSegment: stringParam(params, 'name_segment'),
    page: positiveNumberParam(params, 'page', 1),
    pageSize: positiveNumberParam(params, 'page_size', DEFAULT_PAGE_SIZE),
  };

  const service = new GrpcMarketApiService(token);
  const result = await service.getMarketProducts(filter);

  return NextResponse.json(result);
}
