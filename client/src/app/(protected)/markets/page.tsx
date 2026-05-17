import { cookies } from 'next/headers';
import { Suspense } from 'react';

import GrpcApiService from '@/infrastructure/grpc-api-service';
import GrpcMarketApiService from '@/infrastructure/grpc-market-api-service';
import { MarketFilter } from '@/lib/market-api-service';

import FilterHeader from './components/filter-header';
import ProductGrid from './components/product-grid';

type SearchParams = { [key: string]: string | string[] | undefined };
const PRODUCTS_PAGE_SIZE = 20;

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

  const filter: MarketFilter = {
    brandNameSegment: stringParam(params, 'brand_name'),
    marketName: stringParam(params, 'market_name'),
    nameSegment: stringParam(params, 'name_segment'),
    page: 1,
    pageSize: PRODUCTS_PAGE_SIZE,
  };

  const marketService = new GrpcMarketApiService(token);
  const shoppingService = new GrpcApiService(token);
  const [result, markets, shoppingListSummaries] = await Promise.all([
    marketService.getMarketProducts(filter),
    marketService.getMarkets(),
    shoppingService.getShoppingListSummaries(),
  ]);

  return (
    <>
      <Suspense>
        <FilterHeader marketNames={markets.map(m => m.name)} />
      </Suspense>
      <ProductGrid
        filter={filter}
        initialMarkets={result.markets}
        initialTotalProducts={result.totalProducts}
        shoppingListSummaries={shoppingListSummaries}
      />
    </>
  );
}
