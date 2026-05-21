'use client';

import { usePathname, useRouter } from 'next/navigation';
import { useRef, useState } from 'react';

import { useInfiniteScroll } from '@/lib/hooks/use-infinite-scroll';
import { MarketFilter } from '@/lib/market-api-service';
import { MarketMessage } from '@/lib/market-contracts';

import { useToast } from '../../components/toast-provider';
import { type Product } from './product-card-model';
import { usePaginatedMarketProducts } from './use-paginated-market-products';

export function useProductGridController({
  filter,
  initialMarkets,
  initialTotalProducts,
  isAuthenticated,
}: Readonly<{
  filter: MarketFilter;
  initialMarkets: MarketMessage[];
  initialTotalProducts: number;
  isAuthenticated: boolean;
}>) {
  const pathname = usePathname();
  const router = useRouter();
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
  };
}
