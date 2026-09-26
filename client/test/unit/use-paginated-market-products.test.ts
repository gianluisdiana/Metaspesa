/* eslint-disable @typescript-eslint/no-magic-numbers */
import { PaginatedMarketProductsState } from '@/app/(protected)/markets/components/use-paginated-market-products';
import { describe, expect, it } from 'vitest';

import {
  MarketProductMessage,
  MarketProductsResult,
} from '@/lib/market-contracts';

function product(id: number): MarketProductMessage {
  return {
    brand: 'Brand',
    formats: [],
    id: String(id),
    market: { id: '1', name: 'Market' },
    name: `Product ${id}`,
  };
}

function page(
  number: number,
  totalPages: number,
  ids: number[],
): MarketProductsResult {
  return {
    items: ids.map(product),
    page: number,
    pageSize: 2,
    totalItems: totalPages * 2,
    totalPages,
  };
}

describe('PaginatedMarketProductsState', () => {
  it('uses API pagination metadata to decide whether another page exists', () => {
    const state = PaginatedMarketProductsState.initial(page(1, 3, [1, 2]));
    expect(state.hasMore).toBe(true);
  });

  it('stops after final page', () => {
    const state = PaginatedMarketProductsState.initial(page(3, 3, [5]));
    expect(state.hasMore).toBe(false);
  });

  it('deduplicates by product ID across pages', () => {
    const state = PaginatedMarketProductsState.initial(page(1, 2, [1, 2]));
    const next = state.merge(page(2, 2, [2, 3]));
    expect(next.products.map(item => item.id)).toEqual(['1', '2', '3']);
  });
});
