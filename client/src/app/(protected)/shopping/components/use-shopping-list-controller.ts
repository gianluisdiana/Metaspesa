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
  const [itemPendingDelete, setItemPendingDelete] = useState<{
    name: string;
    productFormatUid: number;
  }>();
  const [temporaryListNamePrompt, setTemporaryListNamePrompt] =
    useState<string>();
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

      if (result.requiresTemporaryListName) {
        setTemporaryListNamePrompt(
          result.message ??
            'Temporary list already exists. Name it and create a new one?',
        );
        return;
      }

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

  async function handleConfirmTemporaryListName(name: string) {
    setIsCreating(true);
    try {
      const result = await client.nameTemporaryListAndCreateNew(name);
      setTemporaryListNamePrompt(undefined);
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
    } finally {
      setIsCreating(false);
    }
  }

  async function handleConfirmDeleteItem() {
    if (!itemPendingDelete) {
      return;
    }

    const previousShoppingList = shoppingList;
    const deletedItemName = itemPendingDelete.name;
    const deletedProductFormatUid = itemPendingDelete.productFormatUid;
    setItemPendingDelete(undefined);
    setShoppingList({
      ...shoppingList,
      products: shoppingList.products.filter(
        product => product.name !== deletedItemName,
      ),
    });

    try {
      const result = await client.removeItem(
        selectedListName,
        deletedProductFormatUid,
      );
      setShoppingList(result.shoppingList);
      setShoppingListSummaries(result.shoppingListSummaries);
      showToast({
        message: `${deletedItemName} deleted.`,
        tone: 'success',
      });
    } catch (requestError) {
      setShoppingList(previousShoppingList);
      showToast({
        message:
          requestError instanceof Error
            ? requestError.message
            : 'Could not delete item.',
        tone: 'error',
      });
    }
  }

  async function handleToggleItemChecked(
    productFormatUid: number,
    checked: boolean,
  ) {
    const previousShoppingList = shoppingList;
    setShoppingList({
      ...shoppingList,
      products: shoppingList.products.map(product =>
        product.productFormatUid === productFormatUid
          ? { ...product, checked }
          : product,
      ),
    });

    try {
      const result = await client.updateItem(
        selectedListName,
        productFormatUid,
        { checked },
      );
      setShoppingList(result.shoppingList);
      setShoppingListSummaries(result.shoppingListSummaries);
    } catch (requestError) {
      setShoppingList(previousShoppingList);
      showToast({
        message:
          requestError instanceof Error
            ? requestError.message
            : 'Could not update item.',
        tone: 'error',
      });
    }
  }

  return {
    handleCancelDeleteItem: () => setItemPendingDelete(undefined),
    handleCancelTemporaryListName: () => setTemporaryListNamePrompt(undefined),
    handleConfirmDeleteItem,
    handleConfirmTemporaryListName,
    handleCreateList,
    handleRequestDeleteItem: (productFormatUid: number, name: string) =>
      setItemPendingDelete({ name, productFormatUid }),
    handleSelectList,
    handleToggleItemChecked,
    hasShoppingLists: shoppingListSummaries.length > 0,
    isCreating,
    isLoading,
    itemPendingDelete: itemPendingDelete?.name,
    tabs: tabsViewModel.tabs,
    temporaryListNamePrompt,
    viewModel,
  };
}
