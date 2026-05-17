'use client';

import { useRef } from 'react';

import { useInfiniteScroll } from '@/lib/hooks/use-infinite-scroll';
import { MarketFilter } from '@/lib/market-api-service';
import { MarketMessage, MarketProductMessage } from '@/lib/market-messages';

import ProductCard, { type Product } from './product-card';
import { usePaginatedMarketProducts } from './use-paginated-market-products';

function toProduct(p: MarketProductMessage): Product {
  const [first] = p.formats;
  return {
    category: p.brandName,
    id: p.name,
    imageAlt: p.name,
    imageUrl: first.imageUrl ?? '',
    name: p.name,
    price: first ? `€${first.price.toFixed(2)}` : '—',
    unit: first?.quantity ?? '',
  };
}

function EmptyState() {
  return (
    <div className="flex flex-col items-center justify-center p-container-margin text-on-surface-variant">
      <span className="material-symbols-outlined text-[48px]">search_off</span>
      <p className="font-body-lg text-body-lg mt-2">No products found</p>
    </div>
  );
}

function MarketSection({ market }: Readonly<{ market: MarketMessage }>) {
  return (
    <section>
      <h2 className="font-title-lg text-title-lg text-on-surface mb-stack-sm">
        {market.name}
      </h2>
      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-gutter">
        {market.products.map(p => (
          <ProductCard key={p.name} product={toProduct(p)} />
        ))}
      </div>
    </section>
  );
}

function LoadingState() {
  return (
    <div className="flex justify-center py-stack-lg text-on-surface-variant">
      <span className="material-symbols-outlined animate-spin text-[28px]">
        progress_activity
      </span>
    </div>
  );
}

export default function ProductGrid({
  filter,
  initialMarkets,
  initialTotalProducts,
}: Readonly<{
  filter: MarketFilter;
  initialMarkets: MarketMessage[];
  initialTotalProducts: number;
}>) {
  const sentinelRef = useRef<HTMLDivElement | null>(null);
  const { hasFailed, hasMore, isLoading, loadNextPage, markets } =
    usePaginatedMarketProducts({
      filter,
      initialMarkets,
      initialTotalProducts,
    });

  useInfiniteScroll({
    hasMore,
    onLoadMore: () => void loadNextPage(),
    sentinelRef,
  });

  if (markets.length === 0) {
    return <EmptyState />;
  }

  return (
    <div className="p-container-margin flex flex-col gap-section-gap">
      {markets.map(market => (
        <MarketSection key={market.name} market={market} />
      ))}
      {isLoading && <LoadingState />}
      {hasFailed && (
        <button
          className="mx-auto rounded-full bg-surface-container px-4 py-2 font-label-md text-label-md text-on-surface hover:bg-surface-container-high"
          type="button"
          onClick={() => void loadNextPage()}
        >
          Retry
        </button>
      )}
      <div ref={sentinelRef} aria-hidden="true" className="h-1" />
    </div>
  );
}
