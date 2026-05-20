import { Suspense } from 'react';

import GrpcApiService from '@/infrastructure/grpc-api-service';
import GrpcMarketApiService from '@/infrastructure/grpc-market-api-service';
import { MarketFilter } from '@/lib/market-api-service';
import { PageSearchParams, stringParam } from '@/lib/search-params';
import { getAuthToken } from '@/lib/server/auth-cookie';

import FilterHeader from './components/filter-header';
import ProductGrid from './components/product-grid';

const PRODUCTS_PAGE_SIZE = 20;

export default async function MarketsPage({
  searchParams,
}: Readonly<{
  searchParams: Promise<PageSearchParams>;
}>) {
  const [params, token] = await Promise.all([searchParams, getAuthToken()]);

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
