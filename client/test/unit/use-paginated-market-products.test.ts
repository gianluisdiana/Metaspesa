/* eslint-disable @typescript-eslint/no-magic-numbers */
import { PaginatedMarketProductsState } from '@/app/(protected)/markets/components/use-paginated-market-products';
import { describe, expect, it } from 'vitest';

import { MarketMessage } from '@/lib/market-contracts';

function market(name: string, productNames: string[]): MarketMessage {
  return {
    name,
    products: productNames.map(productName => ({
      brandName: `${productName} brand`,
      formats: [],
      name: productName,
    })),
  };
}

describe('PaginatedMarketProductsState', () => {
  it('starts from page two after the initial server page', () => {
    const state = PaginatedMarketProductsState.initial(
      [market('A', ['one'])],
      2,
    );

    expect(state.nextPage).toBe(2);
    expect(state.hasMore).toBe(true);
  });

  it('builds the next page URL with filters and default page size', () => {
    const state = PaginatedMarketProductsState.initial([], 10);

    expect(
      state.buildProductsUrl({
        brandNameSegment: 'brand',
        marketName: 'market',
        nameSegment: 'name',
      }),
    ).toBe(
      '/api/markets/products?brand_name=brand&market_name=market&name_segment=name&page=2&page_size=20',
    );
  });

  it('uses an explicit page size when building the next page URL', () => {
    const state = new PaginatedMarketProductsState([], 10, 4);

    expect(state.buildProductsUrl({ pageSize: 5 })).toBe(
      '/api/markets/products?page=4&page_size=5',
    );
  });

  it('deduplicates products when merging more products into an existing market', () => {
    const state = PaginatedMarketProductsState.initial(
      [market('A', ['one', 'two'])],
      4,
    );

    const nextState = state.merge({
      markets: [market('A', ['two', 'three'])],
      totalProducts: 3,
    });

    expect(nextState.markets).toEqual([market('A', ['one', 'two', 'three'])]);
    expect(nextState.nextPage).toBe(3);
    expect(nextState.hasMore).toBe(false);
  });

  it('preserves existing markets and appends new markets', () => {
    const state = PaginatedMarketProductsState.initial(
      [market('A', ['one'])],
      3,
    );

    const nextState = state.merge({
      markets: [market('B', ['two'])],
      totalProducts: 2,
    });

    expect(nextState.markets).toEqual([
      market('A', ['one']),
      market('B', ['two']),
    ]);
  });

  it('resets to initial page state for changed filters', () => {
    const loadedState = new PaginatedMarketProductsState(
      [market('A', ['one', 'two'])],
      4,
      5,
    );

    const resetState = PaginatedMarketProductsState.initial(
      [market('B', ['three'])],
      1,
    );

    expect(loadedState.nextPage).toBe(5);
    expect(resetState.markets).toEqual([market('B', ['three'])]);
    expect(resetState.nextPage).toBe(2);
    expect(resetState.hasMore).toBe(false);
  });
});
