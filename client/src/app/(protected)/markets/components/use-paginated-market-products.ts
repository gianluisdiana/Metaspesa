'use client';

import { useCallback, useEffect, useMemo, useRef, useState } from 'react';

import { MarketFilter } from '@/lib/market-api-service';
import {
  MarketMessage,
  MarketProductMessage,
  MarketProductsResult,
} from '@/lib/market-contracts';

const DEFAULT_PAGE_SIZE = 20;
const FIRST_NEXT_PAGE = 2;

function countProducts(markets: MarketMessage[]): number {
  return markets.reduce((total, market) => total + market.products.length, 0);
}

function productKey(marketName: string, product: MarketProductMessage): string {
  return `${marketName}:${product.name}`;
}

export class PaginatedMarketProductsState {
  public readonly nextPage: number;

  public constructor(
    public readonly markets: MarketMessage[],
    public readonly totalProducts: number,
    nextPage = FIRST_NEXT_PAGE,
  ) {
    this.nextPage = nextPage;
  }

  public static initial(
    markets: MarketMessage[],
    totalProducts: number,
  ): PaginatedMarketProductsState {
    return new PaginatedMarketProductsState(markets, totalProducts);
  }

  public buildProductsUrl(filter: MarketFilter): string {
    const params = new URLSearchParams();
    if (filter.brandNameSegment) {
      params.set('brand_name', filter.brandNameSegment);
    }
    if (filter.marketName) {
      params.set('market_name', filter.marketName);
    }
    if (filter.nameSegment) {
      params.set('name_segment', filter.nameSegment);
    }
    params.set('page', String(this.nextPage));
    params.set('page_size', String(filter.pageSize ?? DEFAULT_PAGE_SIZE));

    return `/api/markets/products?${params}`;
  }

  public get hasMore(): boolean {
    return countProducts(this.markets) < this.totalProducts;
  }

  public merge(result: MarketProductsResult): PaginatedMarketProductsState {
    return new PaginatedMarketProductsState(
      this.mergeMarkets(result.markets),
      result.totalProducts,
      this.nextPage + 1,
    );
  }

  private mergeMarkets(nextMarkets: MarketMessage[]): MarketMessage[] {
    const markets = this.markets.map(market => ({
      ...market,
      products: [...market.products],
    }));
    const marketIndexes = new Map(markets.map((market, i) => [market.name, i]));
    const seenProducts = new Set(
      markets.flatMap(market =>
        market.products.map(product => productKey(market.name, product)),
      ),
    );

    nextMarkets.forEach(nextMarket => {
      const index = marketIndexes.get(nextMarket.name);
      if (index === undefined) {
        marketIndexes.set(nextMarket.name, markets.length);
        nextMarket.products.forEach(product => {
          seenProducts.add(productKey(nextMarket.name, product));
        });
        markets.push({ ...nextMarket, products: [...nextMarket.products] });
        return;
      }

      const products = nextMarket.products.filter(product => {
        const key = productKey(nextMarket.name, product);
        if (seenProducts.has(key)) {
          return false;
        }
        seenProducts.add(key);
        return true;
      });
      markets[index].products.push(...products);
    });

    return markets;
  }
}

export function usePaginatedMarketProducts({
  filter,
  initialMarkets,
  initialTotalProducts,
}: Readonly<{
  filter: MarketFilter;
  initialMarkets: MarketMessage[];
  initialTotalProducts: number;
}>) {
  const [pagination, setPagination] = useState(() =>
    PaginatedMarketProductsState.initial(initialMarkets, initialTotalProducts),
  );
  const [isLoading, setIsLoading] = useState(false);
  const [hasFailed, setHasFailed] = useState(false);
  const isLoadingRef = useRef(false);
  const hasMore = useMemo(() => pagination.hasMore, [pagination]);

  useEffect(() => {
    setPagination(
      PaginatedMarketProductsState.initial(
        initialMarkets,
        initialTotalProducts,
      ),
    );
    isLoadingRef.current = false;
    setIsLoading(false);
    setHasFailed(false);
  }, [initialMarkets, initialTotalProducts]);

  const loadNextPage = useCallback(async () => {
    if (isLoadingRef.current || !hasMore) {
      return;
    }

    isLoadingRef.current = true;
    setIsLoading(true);
    setHasFailed(false);

    try {
      const response = await fetch(pagination.buildProductsUrl(filter), {
        headers: { Accept: 'application/json' },
      });
      if (!response.ok) {
        throw new Error('Failed to load products');
      }
      const result = (await response.json()) as MarketProductsResult;
      setPagination(currentPagination => currentPagination.merge(result));
    } catch {
      setHasFailed(true);
    } finally {
      isLoadingRef.current = false;
      setIsLoading(false);
    }
  }, [filter, hasMore, pagination]);

  return {
    hasFailed,
    hasMore,
    isLoading,
    loadNextPage,
    markets: pagination.markets,
  };
}
