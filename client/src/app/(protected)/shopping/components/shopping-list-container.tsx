'use client';

import { useRouter } from 'next/navigation';
import { useState } from 'react';

import {
  ShoppingListMessage,
  ShoppingListSummaryMessage,
} from '@/lib/messages';
import {
  ShoppingListTabsViewModel,
  ShoppingListViewModel,
} from '@/lib/shopping-list';

import { useToast } from '../../components/toast-provider';
import ListTabs, { ListPageHeader } from './list-header';
import ItemsContainer, { ProgressTracker } from './list-items';
import SummaryFooter from './summary-footer';

type CreateListResponse = {
  message?: string;
  shoppingList: ShoppingListMessage;
  shoppingListSummaries: ShoppingListSummaryMessage[];
};

async function fetchShoppingList(name?: string): Promise<ShoppingListMessage> {
  const params = new URLSearchParams();
  params.set('name', name ?? '');
  const response = await fetch(`/api/shopping/lists?${params}`, {
    cache: 'no-store',
  });
  if (!response.ok) {
    throw new Error('Could not load the current shopping list.');
  }

  return (await response.json()) as ShoppingListMessage;
}

async function createTemporaryList(): Promise<CreateListResponse> {
  const response = await fetch('/api/shopping/lists', {
    method: 'POST',
  });
  const body = (await response.json()) as CreateListResponse;
  if (!response.ok) {
    throw new Error(body.message ?? 'Could not create a temporary list.');
  }

  return body;
}

async function fetchShoppingListSummaries(): Promise<
  ShoppingListSummaryMessage[]
> {
  const response = await fetch('/api/shopping/lists', {
    cache: 'no-store',
  });
  if (!response.ok) {
    throw new Error('Could not load shopping lists.');
  }

  return (await response.json()) as ShoppingListSummaryMessage[];
}

export default function ShoppingListContainer({
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
      setShoppingList(await fetchShoppingList(name));
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
      const result = await createTemporaryList();
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
        setShoppingList(await fetchShoppingList(selectedListName));
        setShoppingListSummaries(await fetchShoppingListSummaries());
      } finally {
        setIsLoading(false);
      }
    } finally {
      setIsCreating(false);
    }
  }

  return (
    <>
      <div className="top-16 z-30 bg-surface/90 backdrop-blur-md border-b border-surface-variant px-container-margin py-stack-md flex flex-col gap-stack-sm shadow-sm shadow-secondary/5">
        <ListPageHeader
          itemCountLabel={viewModel.itemCountLabel}
          listName={viewModel.listName}
        />
        <ListTabs
          isCreating={isCreating}
          onCreateList={handleCreateList}
          onSelectList={handleSelectList}
          tabs={tabsViewModel.tabs}
        />
      </div>
      <div className="p-container-margin pb-36">
        <ProgressTracker progress={viewModel.progress} />
        {isLoading ? (
          <div className="bg-surface-container-lowest border border-surface-container-highest rounded-2xl p-stack-lg text-center mt-stack-md">
            <p className="font-body-md text-body-md text-on-surface-variant">
              Loading current list...
            </p>
          </div>
        ) : (
          <ItemsContainer
            checkedItems={viewModel.checkedItems}
            hasItems={viewModel.hasItems}
            uncheckedSections={viewModel.uncheckedSections}
          />
        )}
      </div>
      <SummaryFooter
        checkedTotal={viewModel.checkedTotal}
        estimatedTotal={viewModel.estimatedTotal}
      />
    </>
  );
}
