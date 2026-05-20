'use client';

import { useRouter } from 'next/navigation';
import { useState } from 'react';

import {
  ShoppingListClient,
  ShoppingListTabsViewModel,
  ShoppingListViewModel,
} from '@/lib/shopping-list';
import {
  ShoppingListMessage,
  ShoppingListSummaryMessage,
} from '@/lib/shopping-list-contracts';

import { useToast } from '../../components/toast-provider';

export function useShoppingListController({
  initialSelectedListName,
  initialShoppingList,
  initialShoppingListSummaries,
}: Readonly<{
  initialSelectedListName?: string;
  initialShoppingList: ShoppingListMessage;
  initialShoppingListSummaries: ShoppingListSummaryMessage[];
}>) {
  const router = useRouter();
  const [shoppingList, setShoppingList] = useState(initialShoppingList);
  const [shoppingListSummaries, setShoppingListSummaries] = useState(
    initialShoppingListSummaries,
  );
  const [selectedListName, setSelectedListName] = useState(
    initialSelectedListName,
  );
  const [isLoading, setIsLoading] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const { showToast } = useToast();
  const client = new ShoppingListClient();
  const viewModel = new ShoppingListViewModel(shoppingList);
  const tabsViewModel = new ShoppingListTabsViewModel(
    shoppingListSummaries,
    selectedListName,
    shoppingList,
  );

  async function handleSelectList(name?: string) {
    setSelectedListName(name);
    setIsLoading(true);
    try {
      setShoppingList(await client.getShoppingList(name));
      router.push(
        name ? `/shopping?name=${encodeURIComponent(name)}` : '/shopping?name=',
      );
    } catch (requestError) {
      showToast({
        message:
          requestError instanceof Error
            ? requestError.message
            : 'Could not load the selected shopping list.',
        tone: 'error',
      });
    } finally {
      setIsLoading(false);
    }
  }

  async function handleCreateList() {
    setIsCreating(true);
    try {
      const result = await client.createTemporaryList();
      setShoppingList(result.shoppingList);
      setShoppingListSummaries(result.shoppingListSummaries);
      setSelectedListName(undefined);
      showToast({
        message: result.message ?? 'Shopping list created.',
        tone: 'success',
      });
      router.push('/shopping');
    } catch (requestError) {
      showToast({
        message:
          requestError instanceof Error
            ? requestError.message
            : 'Could not create a temporary list.',
        tone: 'error',
      });
      setIsLoading(true);
      try {
        setShoppingList(await client.getShoppingList(selectedListName));
        setShoppingListSummaries(await client.getShoppingListSummaries());
      } finally {
        setIsLoading(false);
      }
    } finally {
      setIsCreating(false);
    }
  }

  return {
    handleCreateList,
    handleSelectList,
    isCreating,
    isLoading,
    tabs: tabsViewModel.tabs,
    viewModel,
  };
}
