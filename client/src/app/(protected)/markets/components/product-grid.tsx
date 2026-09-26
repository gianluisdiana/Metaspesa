'use client';

import { useSearchParams } from 'next/navigation';
import { useEffect, useMemo, useState } from 'react';

import RestMarketApiService from '@/infrastructure/rest-market-api-service';
import { MarketFilter } from '@/lib/market-api-service';
import { MarketProductsResult } from '@/lib/market-contracts';
import { ShoppingListSummaryMessage } from '@/lib/shopping-list-contracts';

import { LoadingState, RetryButton } from './product-grid-states';
import { ProductGridView } from './product-grid-view';
import { useProductGridController } from './use-product-grid-controller';

const PAGE_SIZE = 24;

function LoadedProductGrid({
  filter,
  initialPage,
  isAuthenticated,
  shoppingListSummaries,
}: Readonly<{
  filter: MarketFilter;
  initialPage: MarketProductsResult;
  isAuthenticated: boolean;
  shoppingListSummaries: ShoppingListSummaryMessage[];
}>) {
  const controller = useProductGridController({
    filter,
    initialPage,
    initialShoppingListSummaries: shoppingListSummaries,
    isAuthenticated,
  });

  return (
    <ProductGridView
      filter={filter}
      hasFailed={controller.hasFailed}
      isLoading={controller.isLoading}
      isModalOpen={controller.isModalOpen}
      products={controller.products}
      selectedProduct={controller.selectedProduct}
      sentinelRef={controller.sentinelRef}
      shoppingListSummaries={controller.shoppingListSummaries}
      onAddProduct={controller.openAddToListModal}
      onCloseModal={controller.closeAddToListModal}
      onCreateList={controller.handleCreateList}
      onRetry={controller.retry}
      onSelectList={controller.handleSelectList}
    />
  );
}

function MarketProducts({
  filter,
  isAuthenticated,
  shoppingListSummaries,
}: Readonly<{
  filter: MarketFilter;
  isAuthenticated: boolean;
  shoppingListSummaries: ShoppingListSummaryMessage[];
}>) {
  const [page, setPage] = useState<MarketProductsResult>();
  const [error, setError] = useState(false);
  const [retry, setRetry] = useState(0);

  useEffect(() => {
    let cancelled = false;
    new RestMarketApiService().getMarketProducts(filter).then(
      result => {
        if (!cancelled) setPage(result);
      },
      () => {
        if (!cancelled) setError(true);
      },
    );
    return () => {
      cancelled = true;
    };
  }, [filter, retry]);

  if (error) {
    return (
      <RetryButton
        onRetry={() => {
          setError(false);
          setRetry(value => value + 1);
        }}
      />
    );
  }
  if (!page) {
    return <LoadingState />;
  }
  return (
    <LoadedProductGrid
      filter={filter}
      initialPage={page}
      isAuthenticated={isAuthenticated}
      shoppingListSummaries={shoppingListSummaries}
    />
  );
}

export default function ProductGrid({
  isAuthenticated,
  shoppingListSummaries,
}: Readonly<{
  isAuthenticated: boolean;
  shoppingListSummaries: ShoppingListSummaryMessage[];
}>) {
  const params = useSearchParams();
  const marketIds = params
    .getAll('marketId')
    .map(Number)
    .filter(id => Number.isSafeInteger(id) && id > 0)
    .join(',');
  const query = params.get('query') ?? '';
  const brand = params.get('brand') ?? '';
  const sortParam = params.get('sort');
  const sort =
    sortParam === 'priceAsc' || sortParam === 'priceDesc' ? sortParam : 'name';
  const filter: MarketFilter = useMemo(
    () => ({
      brand: brand || undefined,
      marketId: marketIds ? marketIds.split(',') : undefined,
      page: 1,
      pageSize: PAGE_SIZE,
      query: query || undefined,
      sort,
    }),
    [brand, marketIds, query, sort],
  );

  return (
    <MarketProducts
      key={JSON.stringify(filter)}
      filter={filter}
      isAuthenticated={isAuthenticated}
      shoppingListSummaries={shoppingListSummaries}
    />
  );
}
