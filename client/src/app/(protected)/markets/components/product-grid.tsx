'use client';

import { MarketFilter } from '@/lib/market-api-service';
import { MarketMessage } from '@/lib/market-contracts';
import { ShoppingListSummaryMessage } from '@/lib/shopping-list-contracts';

import { ProductGridView } from './product-grid-view';
import { useProductGridController } from './use-product-grid-controller';

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
  const controller = useProductGridController({
    filter,
    initialMarkets,
    initialTotalProducts,
  });

  return (
    <ProductGridView
      filter={filter}
      hasFailed={controller.hasFailed}
      isLoading={controller.isLoading}
      isModalOpen={controller.isModalOpen}
      markets={controller.markets}
      selectedProduct={controller.selectedProduct}
      sentinelRef={controller.sentinelRef}
      shoppingListSummaries={shoppingListSummaries}
      onAddProduct={controller.openAddToListModal}
      onCloseModal={controller.closeAddToListModal}
      onCreateList={controller.handleCreateList}
      onRetry={controller.retry}
      onSelectList={controller.handleSelectList}
    />
  );
}
