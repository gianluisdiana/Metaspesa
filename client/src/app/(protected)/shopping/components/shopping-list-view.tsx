import {
  ShoppingListTabViewModel,
  ShoppingListViewModel,
} from '@/lib/shopping-list';

import { DeleteItemConfirmationModal } from './delete-item-confirmation-modal';
import ListTabs, { ListPageHeader } from './list-header';
import ItemsContainer from './list-items';
import { NoShoppingLists } from './no-shopping-lists';
import { ProgressTracker } from './progress-tracker';
import { ShoppingListLoadingState } from './shopping-list-loading-state';
import SummaryFooter from './summary-footer';
import { TemporaryListNameModal } from './temporary-list-name-modal';

export function ShoppingListView({
  isCreating,
  hasShoppingLists,
  isLoading,
  itemPendingDelete,
  onCancelDeleteItem,
  onCancelTemporaryListName,
  onConfirmDeleteItem,
  onConfirmTemporaryListName,
  onCreateList,
  onRequestDeleteItem,
  onSelectList,
  onToggleItemChecked,
  tabs,
  temporaryListNamePrompt,
  viewModel,
}: Readonly<{
  isCreating: boolean;
  hasShoppingLists: boolean;
  isLoading: boolean;
  itemPendingDelete?: string;
  onCancelDeleteItem: () => void;
  onCancelTemporaryListName: () => void;
  onConfirmDeleteItem: () => void;
  onConfirmTemporaryListName: (name: string) => void;
  onCreateList: () => void;
  onRequestDeleteItem: (productFormatUid: string, itemName: string) => void;
  onSelectList: (id: string) => void;
  onToggleItemChecked: (productFormatUid: string, checked: boolean) => void;
  tabs: ShoppingListTabViewModel[];
  temporaryListNamePrompt?: string;
  viewModel: ShoppingListViewModel;
}>) {
  if (!hasShoppingLists) {
    return (
      <>
        <NoShoppingLists isCreating={isCreating} onCreateList={onCreateList} />
        {temporaryListNamePrompt && (
          <TemporaryListNameModal
            isSaving={isCreating}
            message={temporaryListNamePrompt}
            onCancel={onCancelTemporaryListName}
            onConfirm={onConfirmTemporaryListName}
          />
        )}
      </>
    );
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
          onCreateList={onCreateList}
          onSelectList={onSelectList}
          tabs={tabs}
        />
      </div>
      <div className="p-container-margin pb-36">
        <ProgressTracker progress={viewModel.progress} />
        {isLoading ? (
          <ShoppingListLoadingState />
        ) : (
          <ItemsContainer
            checkedItems={viewModel.checkedItems}
            hasItems={viewModel.hasItems}
            uncheckedSections={viewModel.uncheckedSections}
            onRequestDeleteItem={onRequestDeleteItem}
            onToggleItemChecked={onToggleItemChecked}
          />
        )}
      </div>
      <SummaryFooter
        checkedTotal={viewModel.checkedTotal}
        estimatedTotal={viewModel.estimatedTotal}
      />
      {itemPendingDelete && (
        <DeleteItemConfirmationModal
          itemName={itemPendingDelete}
          onCancel={onCancelDeleteItem}
          onConfirm={onConfirmDeleteItem}
        />
      )}
      {temporaryListNamePrompt && (
        <TemporaryListNameModal
          isSaving={isCreating}
          message={temporaryListNamePrompt}
          onCancel={onCancelTemporaryListName}
          onConfirm={onConfirmTemporaryListName}
        />
      )}
    </>
  );
}
