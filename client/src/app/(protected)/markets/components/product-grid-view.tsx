import { RefObject } from 'react';

import { MarketFilter } from '@/lib/market-api-service';
import { MarketProductMessage } from '@/lib/market-contracts';
import { ShoppingListSummaryMessage } from '@/lib/shopping-list-contracts';

import AddToListModal from './add-to-list-modal';
import { MarketSection } from './market-section';
import { type Product } from './product-card-model';
import { EmptyState, LoadingState, RetryButton } from './product-grid-states';

export function ProductGridView({
  filter,
  hasFailed,
  isLoading,
  isModalOpen,
  products,
  onAddProduct,
  onCloseModal,
  onCreateList,
  onRetry,
  onSelectList,
  selectedProduct,
  sentinelRef,
  shoppingListSummaries,
}: Readonly<{
  filter: MarketFilter;
  hasFailed: boolean;
  isLoading: boolean;
  isModalOpen: boolean;
  products: MarketProductMessage[];
  onAddProduct: (product: Product) => void;
  onCloseModal: () => void;
  onCreateList: () => void;
  onRetry: () => void;
  onSelectList: (listId: string) => void;
  selectedProduct?: Product;
  sentinelRef: RefObject<HTMLDivElement | null>;
  shoppingListSummaries: ShoppingListSummaryMessage[];
}>) {
  if (products.length === 0) {
    return <EmptyState />;
  }

  const grouped = new Map<
    string,
    { name: string; products: MarketProductMessage[] }
  >();
  products.forEach(product => {
    const group = grouped.get(product.market.id);
    if (group) {
      group.products.push(product);
    } else {
      grouped.set(product.market.id, {
        name: product.market.name,
        products: [product],
      });
    }
  });

  return (
    <>
      <div className="p-container-margin flex flex-col gap-section-gap">
        {[...grouped.entries()].map(([marketId, market]) => (
          <MarketSection
            key={marketId}
            marketName={market.name}
            products={market.products}
            showDivider={!filter.marketId?.length}
            onAddProduct={onAddProduct}
          />
        ))}
        {isLoading && <LoadingState />}
        {hasFailed && <RetryButton onRetry={onRetry} />}
        <div ref={sentinelRef} aria-hidden="true" className="h-1" />
      </div>
      <AddToListModal
        isOpen={isModalOpen}
        productName={selectedProduct?.name}
        shoppingListSummaries={shoppingListSummaries}
        onClose={onCloseModal}
        onCreateList={onCreateList}
        onSelectList={onSelectList}
      />
    </>
  );
}
