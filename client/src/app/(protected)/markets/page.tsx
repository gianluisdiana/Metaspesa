import { cookies } from 'next/headers';
import { Suspense } from 'react';

import GrpcMarketApiService from '@/infrastructure/grpc-market-api-service';

import FilterHeader from './components/filter-header';
import ProductGrid from './components/product-grid';

type SearchParams = { [key: string]: string | string[] | undefined };

function stringParam(params: SearchParams, key: string): string | undefined {
  const value = params[key];
  return typeof value === 'string' && value.length > 0 ? value : undefined;
}

export default async function MarketsPage({
  searchParams,
}: Readonly<{
  searchParams: Promise<SearchParams>;
}>) {
  const [params, cookieStore] = await Promise.all([searchParams, cookies()]);
  const token = cookieStore.get('auth_token')?.value ?? '';

  const filter = {
    brandName: stringParam(params, 'brand_name'),
    marketName: stringParam(params, 'market_name'),
    nameSegment: stringParam(params, 'name_segment'),
  };

  const service = new GrpcMarketApiService(token);
  const [result, markets] = await Promise.all([
    service.getMarketProducts(filter),
    service.getMarkets(),
  ]);

  return (
    <>
      <Suspense>
        <FilterHeader marketNames={markets.map(m => m.name)} />
      </Suspense>
      <ProductGrid markets={result.markets} />
    </>
  );
}
