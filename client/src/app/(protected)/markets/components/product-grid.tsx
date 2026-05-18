'use client';

import { useRef, useState } from 'react';

import { useInfiniteScroll } from '@/lib/hooks/use-infinite-scroll';
import { MarketFilter } from '@/lib/market-api-service';
import { MarketMessage, MarketProductMessage } from '@/lib/market-contracts';
import { ShoppingListSummaryMessage } from '@/lib/shopping-list-contracts';

import { useToast } from '../../components/toast-provider';
import AddToListModal from './add-to-list-modal';
import ProductCard, { type Product } from './product-card';
import { usePaginatedMarketProducts } from './use-paginated-market-products';

const EURO_SYMBOL = '€';
const MISSING_PRICE_LABEL = '—';

function toProduct(p: MarketProductMessage): Product {
  const [first] = p.formats;
  return {
    category: p.brandName,
    id: p.name,
    imageAlt: p.name,
    imageUrl: first.imageUrl ?? '',
    name: p.name,
    price: first
      ? `${EURO_SYMBOL}${first.price.toFixed(2)}`
      : MISSING_PRICE_LABEL,
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

function MarketDivider({ marketName }: Readonly<{ marketName: string }>) {
  return (
    <div className="flex items-center gap-4">
      <h2 className="font-headline-md text-headline-md whitespace-nowrap text-on-surface">
        {marketName}
      </h2>
      <div className="h-px flex-1 bg-outline-variant" />
    </div>
  );
}

function MarketSection({
  market,
  onAddProduct,
  showDivider,
}: Readonly<{
  market: MarketMessage;
  onAddProduct: (product: Product) => void;
  showDivider: boolean;
}>) {
  return (
    <section className="flex flex-col gap-stack-sm">
      {showDivider && <MarketDivider marketName={market.name} />}
      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-gutter">
        {market.products.map(p => (
          <ProductCard
            key={p.name}
            product={toProduct(p)}
            onAdd={() => onAddProduct(toProduct(p))}
          />
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
  shoppingListSummaries,
}: Readonly<{
  filter: MarketFilter;
  initialMarkets: MarketMessage[];
  initialTotalProducts: number;
  shoppingListSummaries: ShoppingListSummaryMessage[];
}>) {
  const sentinelRef = useRef<HTMLDivElement | null>(null);
  const [selectedProduct, setSelectedProduct] = useState<Product>();
  const { showToast } = useToast();
  const { hasFailed, hasMore, isLoading, loadNextPage, markets } =
    usePaginatedMarketProducts({
      filter,
      initialMarkets,
      initialTotalProducts,
    });
  const [isModalOpen, setIsModalOpen] = useState(false);

  useInfiniteScroll({
    hasMore,
    onLoadMore: () => void loadNextPage(),
    sentinelRef,
  });

  if (markets.length === 0) {
    return <EmptyState />;
  }

  function openAddToListModal(product: Product) {
    setSelectedProduct(product);
    setIsModalOpen(true);
  }

  function closeAddToListModal() {
    setIsModalOpen(false);
  }

  function handleCreateList() {
    closeAddToListModal();
    showToast({
      message: 'Create shopping list flow is not connected yet.',
      tone: 'info',
    });
  }

  function handleSelectList(listName?: string) {
    closeAddToListModal();
    showToast({
      message: `Item queued for ${listName ?? 'Temporary List'}.`,
      tone: 'success',
    });
  }

  return (
    <>
      <div className="p-container-margin flex flex-col gap-section-gap">
        {markets.map(market => (
          <MarketSection
            key={market.name}
            market={market}
            showDivider={!filter.marketName}
            onAddProduct={openAddToListModal}
          />
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
      <AddToListModal
        isOpen={isModalOpen}
        productName={selectedProduct?.name}
        shoppingListSummaries={shoppingListSummaries}
        onClose={closeAddToListModal}
        onCreateList={handleCreateList}
        onSelectList={handleSelectList}
      />
    </>
  );
}
