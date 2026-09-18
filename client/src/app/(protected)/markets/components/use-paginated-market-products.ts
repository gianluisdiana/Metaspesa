'use client';

import { useCallback, useRef, useState } from 'react';

import RestMarketApiService from '@/infrastructure/rest-market-api-service';
import { MarketFilter } from '@/lib/market-api-service';
import {
  MarketProductMessage,
  MarketProductsResult,
} from '@/lib/market-contracts';

export class PaginatedMarketProductsState {
  public constructor(
    public readonly products: MarketProductMessage[],
    public readonly nextPage: number,
    public readonly totalPages: number,
  ) {}

  public static initial(
    page: MarketProductsResult,
  ): PaginatedMarketProductsState {
    return new PaginatedMarketProductsState(
      page.items,
      page.page + 1,
      page.totalPages,
    );
  }

  public get hasMore(): boolean {
    return this.nextPage <= this.totalPages;
  }

  public merge(page: MarketProductsResult): PaginatedMarketProductsState {
    const seen = new Set(this.products.map(product => product.id));
    return new PaginatedMarketProductsState(
      [
        ...this.products,
        ...page.items.filter(product => !seen.has(product.id)),
      ],
      page.page + 1,
      page.totalPages,
    );
  }
}

export function usePaginatedMarketProducts({
  filter,
  initialPage,
}: Readonly<{
  filter: MarketFilter;
  initialPage: MarketProductsResult;
}>) {
  const [pagination, setPagination] = useState(() =>
    PaginatedMarketProductsState.initial(initialPage),
  );
  const [isLoading, setIsLoading] = useState(false);
  const [hasFailed, setHasFailed] = useState(false);
  const isLoadingRef = useRef(false);

  const loadNextPage = useCallback(async () => {
    if (isLoadingRef.current || !pagination.hasMore) return;
    isLoadingRef.current = true;
    setIsLoading(true);
    setHasFailed(false);
    try {
      const page = await new RestMarketApiService().getMarketProducts({
        ...filter,
        page: pagination.nextPage,
      });
      setPagination(current => current.merge(page));
    } catch {
      setHasFailed(true);
    } finally {
      isLoadingRef.current = false;
      setIsLoading(false);
    }
  }, [filter, pagination]);

  return {
    hasFailed,
    hasMore: pagination.hasMore,
    isLoading,
    loadNextPage,
    products: pagination.products,
  };
}
