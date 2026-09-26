'use client';

import {
  ShoppingListMessage,
  ShoppingListSummaryMessage,
} from '@/lib/shopping-list-contracts';

import { ShoppingListView } from './shopping-list-view';
import { useShoppingListController } from './use-shopping-list-controller';

export default function ShoppingListContainer({
  initialSelectedListId,
  initialShoppingList,
  initialShoppingListSummaries,
}: Readonly<{
  initialSelectedListId?: string;
  initialShoppingList: ShoppingListMessage;
  initialShoppingListSummaries: ShoppingListSummaryMessage[];
}>) {
  const controller = useShoppingListController({
    initialSelectedListId,
    initialShoppingList,
    initialShoppingListSummaries,
  });

  return (
    <ShoppingListView
      hasShoppingLists={controller.hasShoppingLists}
      isCreating={controller.isCreating}
      isLoading={controller.isLoading}
      itemPendingDelete={controller.itemPendingDelete}
      tabs={controller.tabs}
      viewModel={controller.viewModel}
      onCancelDeleteItem={controller.handleCancelDeleteItem}
      onCancelTemporaryListName={controller.handleCancelTemporaryListName}
      onConfirmDeleteItem={controller.handleConfirmDeleteItem}
      onConfirmTemporaryListName={controller.handleConfirmTemporaryListName}
      onCreateList={controller.handleCreateList}
      onRequestDeleteItem={controller.handleRequestDeleteItem}
      onSelectList={controller.handleSelectList}
      onToggleItemChecked={controller.handleToggleItemChecked}
      temporaryListNamePrompt={controller.temporaryListNamePrompt}
    />
  );
}
