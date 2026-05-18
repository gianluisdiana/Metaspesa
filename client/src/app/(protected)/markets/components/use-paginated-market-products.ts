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

function mergeMarkets(
  currentMarkets: MarketMessage[],
  nextMarkets: MarketMessage[],
): MarketMessage[] {
  const markets = currentMarkets.map(market => ({
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

function buildProductsUrl(filter: MarketFilter, nextPage: number): string {
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
  params.set('page', String(nextPage));
  params.set('page_size', String(filter.pageSize ?? DEFAULT_PAGE_SIZE));

  return `/api/markets/products?${params}`;
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
  const [markets, setMarkets] = useState(initialMarkets);
  const [totalProducts, setTotalProducts] = useState(initialTotalProducts);
  const [nextPage, setNextPage] = useState(FIRST_NEXT_PAGE);
  const [isLoading, setIsLoading] = useState(false);
  const [hasFailed, setHasFailed] = useState(false);
  const isLoadingRef = useRef(false);
  const loadedProducts = useMemo(() => countProducts(markets), [markets]);
  const hasMore = loadedProducts < totalProducts;

  useEffect(() => {
    setMarkets(initialMarkets);
    setTotalProducts(initialTotalProducts);
    setNextPage(FIRST_NEXT_PAGE);
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
      const response = await fetch(buildProductsUrl(filter, nextPage), {
        headers: { Accept: 'application/json' },
      });
      if (!response.ok) {
        throw new Error('Failed to load products');
      }
      const result = (await response.json()) as MarketProductsResult;
      setMarkets(currentMarkets =>
        mergeMarkets(currentMarkets, result.markets),
      );
      setTotalProducts(result.totalProducts);
      setNextPage(page => page + 1);
    } catch {
      setHasFailed(true);
    } finally {
      isLoadingRef.current = false;
      setIsLoading(false);
    }
  }, [filter, hasMore, nextPage]);

  return {
    hasFailed,
    hasMore,
    isLoading,
    loadNextPage,
    markets,
  };
}
