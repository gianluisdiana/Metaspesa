'use client';

import {
  ShoppingListMessage,
  ShoppingListSummaryMessage,
} from '@/lib/shopping-list-contracts';

import { ShoppingListView } from './shopping-list-view';
import { useShoppingListController } from './use-shopping-list-controller';

export default function ShoppingListContainer({
  initialSelectedListName,
  initialShoppingList,
  initialShoppingListSummaries,
}: Readonly<{
  initialSelectedListName?: string;
  initialShoppingList: ShoppingListMessage;
  initialShoppingListSummaries: ShoppingListSummaryMessage[];
}>) {
  const controller = useShoppingListController({
    initialSelectedListName,
    initialShoppingList,
    initialShoppingListSummaries,
  });

  return (
    <ShoppingListView
      isCreating={controller.isCreating}
      isLoading={controller.isLoading}
      tabs={controller.tabs}
      viewModel={controller.viewModel}
      onCreateList={controller.handleCreateList}
      onSelectList={controller.handleSelectList}
    />
  );
}
