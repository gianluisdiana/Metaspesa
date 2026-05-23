'use client';

import { usePathname, useRouter } from 'next/navigation';
import { useRef, useState } from 'react';

import { useInfiniteScroll } from '@/lib/hooks/use-infinite-scroll';
import { MarketFilter } from '@/lib/market-api-service';
import { MarketMessage } from '@/lib/market-contracts';
import { ShoppingListClient } from '@/lib/shopping-list';
import { ShoppingListSummaryMessage } from '@/lib/shopping-list-contracts';

import { useToast } from '../../components/toast-provider';
import { type Product } from './product-card-model';
import { usePaginatedMarketProducts } from './use-paginated-market-products';

export function useProductGridController({
  filter,
  initialMarkets,
  initialShoppingListSummaries,
  initialTotalProducts,
  isAuthenticated,
}: Readonly<{
  filter: MarketFilter;
  initialMarkets: MarketMessage[];
  initialShoppingListSummaries: ShoppingListSummaryMessage[];
  initialTotalProducts: number;
  isAuthenticated: boolean;
}>) {
  const pathname = usePathname();
  const router = useRouter();
  const sentinelRef = useRef<HTMLDivElement | null>(null);
  const [selectedProduct, setSelectedProduct] = useState<Product>();
  const [shoppingListSummaries, setShoppingListSummaries] = useState(
    initialShoppingListSummaries,
  );
  const { showToast } = useToast();
  const client = new ShoppingListClient();
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

  function openAddToListModal(product: Product) {
    if (!isAuthenticated) {
      showToast({
        message: 'Log in to add products to a shopping list.',
        tone: 'info',
      });
      router.push(`/auth/login?next=${encodeURIComponent(pathname)}`);
      return;
    }

    setSelectedProduct(product);
    setIsModalOpen(true);
  }

  function closeAddToListModal() {
    setIsModalOpen(false);
  }

  async function addSelectedProductToList(listName?: string) {
    if (!selectedProduct) {
      return;
    }

    const result = await client.addItemsToList(listName, [
      {
        checked: false,
        name: selectedProduct.name,
        price: selectedProduct.priceValue,
        quantity: selectedProduct.unit || undefined,
      },
    ]);
    setShoppingListSummaries(result.shoppingListSummaries);
    showToast({
      message: `Item added to ${listName ?? 'Temporary List'}.`,
      tone: 'success',
    });
  }

  async function handleCreateList() {
    closeAddToListModal();
    try {
      const result = await client.createTemporaryList();
      setShoppingListSummaries(result.shoppingListSummaries);
      await addSelectedProductToList(undefined);
    } catch (requestError) {
      showToast({
        message:
          requestError instanceof Error
            ? requestError.message
            : 'Could not add item to shopping list.',
        tone: 'error',
      });
    }
  }

  async function handleSelectList(listName?: string) {
    closeAddToListModal();
    try {
      await addSelectedProductToList(listName);
    } catch (requestError) {
      showToast({
        message:
          requestError instanceof Error
            ? requestError.message
            : 'Could not add item to shopping list.',
        tone: 'error',
      });
    }
  }

  return {
    closeAddToListModal,
    handleCreateList,
    handleSelectList,
    hasFailed,
    isLoading,
    isModalOpen,
    markets,
    openAddToListModal,
    retry: () => void loadNextPage(),
    selectedProduct,
    sentinelRef,
    shoppingListSummaries,
  };
}
