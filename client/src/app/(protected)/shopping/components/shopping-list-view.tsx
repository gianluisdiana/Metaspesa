import {
  ShoppingListTabViewModel,
  ShoppingListViewModel,
} from '@/lib/shopping-list';

import ListTabs, { ListPageHeader } from './list-header';
import ItemsContainer from './list-items';
import { ProgressTracker } from './progress-tracker';
import { ShoppingListLoadingState } from './shopping-list-loading-state';
import SummaryFooter from './summary-footer';

export function ShoppingListView({
  isCreating,
  isLoading,
  onCreateList,
  onSelectList,
  tabs,
  viewModel,
}: Readonly<{
  isCreating: boolean;
  isLoading: boolean;
  onCreateList: () => void;
  onSelectList: (name?: string) => void;
  tabs: ShoppingListTabViewModel[];
  viewModel: ShoppingListViewModel;
}>) {
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
