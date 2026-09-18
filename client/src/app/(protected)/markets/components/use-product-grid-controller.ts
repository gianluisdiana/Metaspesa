'use client';

import { usePathname, useRouter } from 'next/navigation';
import { useRef, useState } from 'react';

import { useInfiniteScroll } from '@/lib/hooks/use-infinite-scroll';
import { MarketFilter } from '@/lib/market-api-service';
import { MarketProductsResult } from '@/lib/market-contracts';
import { ShoppingListClient } from '@/lib/shopping-list';
import { ShoppingListSummaryMessage } from '@/lib/shopping-list-contracts';

import { useToast } from '../../components/toast-provider';
import { type Product } from './product-card-model';
import { usePaginatedMarketProducts } from './use-paginated-market-products';

export function useProductGridController({
  filter,
  initialPage,
  initialShoppingListSummaries,
  isAuthenticated,
}: Readonly<{
  filter: MarketFilter;
  initialPage: MarketProductsResult;
  initialShoppingListSummaries: ShoppingListSummaryMessage[];
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
  const { hasFailed, hasMore, isLoading, loadNextPage, products } =
    usePaginatedMarketProducts({
      filter,
      initialPage,
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
    if (selectedProduct.productFormatUid === undefined) {
      throw new Error('Product format UID is required to add an item.');
    }

    const result = await client.addItemsToList(listName, [
      {
        checked: false,
        name: selectedProduct.name,
        price: selectedProduct.priceValue,
        productFormatUid: selectedProduct.productFormatUid,
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
    openAddToListModal,
    products,
    retry: () => void loadNextPage(),
    selectedProduct,
    sentinelRef,
    shoppingListSummaries,
  };
}
