import { RefObject } from 'react';

import { MarketFilter } from '@/lib/market-api-service';
import { MarketMessage } from '@/lib/market-contracts';
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
  markets,
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
  markets: MarketMessage[];
  onAddProduct: (product: Product) => void;
  onCloseModal: () => void;
  onCreateList: () => void;
  onRetry: () => void;
  onSelectList: (listName?: string) => void;
  selectedProduct?: Product;
  sentinelRef: RefObject<HTMLDivElement | null>;
  shoppingListSummaries: ShoppingListSummaryMessage[];
}>) {
  if (markets.length === 0) {
    return <EmptyState />;
  }

  return (
    <>
      <div className="p-container-margin flex flex-col gap-section-gap">
        {markets.map(market => (
          <MarketSection
            key={market.name}
            market={market}
            showDivider={!filter.marketName}
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
