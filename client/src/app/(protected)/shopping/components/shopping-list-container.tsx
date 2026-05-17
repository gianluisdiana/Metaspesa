'use client';

import { useState } from 'react';

import { ShoppingListMessage } from '@/lib/messages';
import { ShoppingListViewModel } from '@/lib/shopping-list-view-model';

import ListTabs, { ListPageHeader } from './list-header';
import ItemsContainer, { ProgressTracker } from './list-items';
import SummaryFooter from './summary-footer';

type CreateListResponse = {
  message?: string;
  shoppingList: ShoppingListMessage;
};

async function fetchShoppingList(name?: string): Promise<ShoppingListMessage> {
  const params = new URLSearchParams();
  if (name) {
    params.set('name', name);
  }
  const query = params.size > 0 ? `?${params}` : '';
  const response = await fetch(`/api/shopping/lists${query}`, {
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

export default function ShoppingListContainer({
  initialSelectedListName,
  initialShoppingList,
}: Readonly<{
  initialSelectedListName?: string;
  initialShoppingList: ShoppingListMessage;
}>) {
  const [shoppingList, setShoppingList] = useState(initialShoppingList);
  const [selectedListName] = useState(initialSelectedListName);
  const [isLoading, setIsLoading] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const [error, setError] = useState<string>();
  const viewModel = new ShoppingListViewModel(shoppingList);

  async function handleCreateList() {
    setIsCreating(true);
    setError(undefined);
    try {
      const result = await createTemporaryList();
      setShoppingList(result.shoppingList);
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : 'Could not create a temporary list.',
      );
      setIsLoading(true);
      try {
        setShoppingList(await fetchShoppingList(selectedListName));
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
        <ListTabs isCreating={isCreating} onCreateList={handleCreateList} />
      </div>
      <div className="p-container-margin pb-36">
        <ProgressTracker progress={viewModel.progress} />
        {error && (
          <p className="font-label-md text-label-md text-error mt-stack-sm">
            {error}
          </p>
        )}
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
