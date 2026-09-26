'use client';

import { useRouter } from 'next/navigation';
import { useState } from 'react';

import { ShoppingApiError } from '@/infrastructure/rest-shopping-api-service';
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
  initialSelectedListId,
  initialShoppingList,
  initialShoppingListSummaries,
}: Readonly<{
  initialSelectedListId?: string;
  initialShoppingList: ShoppingListMessage;
  initialShoppingListSummaries: ShoppingListSummaryMessage[];
}>) {
  const router = useRouter();
  const [shoppingList, setShoppingList] = useState(initialShoppingList);
  const [shoppingListSummaries, setShoppingListSummaries] = useState(
    initialShoppingListSummaries,
  );
  const [selectedListId, setSelectedListId] = useState(initialSelectedListId);
  const [isLoading, setIsLoading] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const [itemPendingDelete, setItemPendingDelete] = useState<{
    name: string;
    productFormatUid: string;
  }>();
  const [temporaryListNamePrompt, setTemporaryListNamePrompt] =
    useState<string>();
  const { showToast } = useToast();
  const client = new ShoppingListClient();
  const viewModel = new ShoppingListViewModel(shoppingList);
  const tabsViewModel = new ShoppingListTabsViewModel(
    shoppingListSummaries,
    selectedListId,
    shoppingList,
  );

  async function refreshList(id: string) {
    const [list, summaries] = await Promise.all([
      client.getShoppingList(id),
      client.getShoppingListSummaries(),
    ]);
    setShoppingList(list);
    setShoppingListSummaries(summaries);
  }

  async function handleSelectList(id: string) {
    setIsLoading(true);
    try {
      setShoppingList(await client.getShoppingList(id));
      setSelectedListId(id);
      router.push(`/shopping?listId=${id}`);
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
      const id = await client.createShoppingList();
      await refreshList(id);
      setSelectedListId(id);
      router.push(`/shopping?listId=${id}`);
      showToast({ message: 'Shopping list created.', tone: 'success' });
    } catch (requestError) {
      if (
        requestError instanceof ShoppingApiError &&
        requestError.code === 'ShoppingList.Temporary.AlreadyExists'
      ) {
        setShoppingListSummaries(await client.getShoppingListSummaries());
        setTemporaryListNamePrompt(
          'Temporary list already exists. Name it and create a new one?',
        );
      } else {
        showToast({
          message:
            requestError instanceof Error
              ? requestError.message
              : 'Could not create a temporary list.',
          tone: 'error',
        });
      }
    } finally {
      setIsCreating(false);
    }
  }

  async function handleConfirmTemporaryListName(name: string) {
    const temporary = shoppingListSummaries.find(
      summary => summary.isTemporary,
    );
    if (!temporary?.id) {
      showToast({ message: 'Temporary list was not found.', tone: 'error' });
      return;
    }
    setIsCreating(true);
    try {
      await client.renameShoppingList(temporary.id, name);
      const id = await client.createShoppingList();
      await refreshList(id);
      setSelectedListId(id);
      setTemporaryListNamePrompt(undefined);
      router.push(`/shopping?listId=${id}`);
      showToast({ message: 'Shopping list created.', tone: 'success' });
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
    if (!itemPendingDelete || !selectedListId) {
      return;
    }
    const previousShoppingList = shoppingList;
    const { name, productFormatUid } = itemPendingDelete;
    setItemPendingDelete(undefined);
    setShoppingList({
      ...shoppingList,
      products: shoppingList.products.filter(
        product => product.productFormatUid !== productFormatUid,
      ),
    });
    try {
      await client.removeItem(selectedListId, productFormatUid);
      await refreshList(selectedListId);
      showToast({ message: `${name} deleted.`, tone: 'success' });
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
    productFormatUid: string,
    checked: boolean,
  ) {
    if (!selectedListId) return;
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
      await client.updateItem(selectedListId, productFormatUid, { checked });
      await refreshList(selectedListId);
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
    handleRequestDeleteItem: (productFormatUid: string, name: string) =>
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
