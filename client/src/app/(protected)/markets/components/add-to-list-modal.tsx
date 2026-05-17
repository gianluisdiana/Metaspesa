'use client';

import { useState } from 'react';

import { ShoppingListSummaryMessage } from '@/lib/messages';

type Props = {
  isOpen: boolean;
  productName?: string;
  shoppingListSummaries: ShoppingListSummaryMessage[];
  onClose: () => void;
  onCreateList: () => void;
  onSelectList: (listName?: string) => void;
};

function listLabel(summary: ShoppingListSummaryMessage): string {
  return summary.name && summary.name.length > 0
    ? summary.name
    : 'Temporary List';
}

export default function AddToListModal({
  isOpen,
  onClose,
  onCreateList,
  onSelectList,
  productName,
  shoppingListSummaries,
}: Readonly<Props>) {
  const [selectedListName, setSelectedListName] = useState<string>();
  const hasShoppingLists = shoppingListSummaries.length > 0;
  const selectedName = selectedListName ?? shoppingListSummaries[0]?.name;

  if (!isOpen) {
    return null;
  }

  return (
    <div
      className="fixed inset-0 z-[100] flex items-center justify-center bg-on-surface/40 p-4 backdrop-blur-sm"
      role="presentation"
      onClick={onClose}
    >
      <div
        aria-modal="true"
        className="flex w-full max-w-sm flex-col gap-6 rounded-2xl bg-surface-container-lowest p-6 shadow-2xl"
        role="dialog"
        onClick={event => event.stopPropagation()}
      >
        <div className="flex items-center justify-between gap-4">
          <div>
            <h3 className="font-headline-md text-headline-md text-on-surface">
              Add to List
            </h3>
            {productName && (
              <p className="mt-1 font-body-md text-sm text-on-surface-variant">
                {productName}
              </p>
            )}
          </div>
          <button
            aria-label="Close add to list modal"
            className="flex size-8 items-center justify-center rounded-full text-outline transition-colors hover:bg-surface-container hover:text-on-surface"
            type="button"
            onClick={onClose}
          >
            <span className="material-symbols-outlined">close</span>
          </button>
        </div>

        {hasShoppingLists ? (
          <div className="flex max-h-75 flex-col gap-2 overflow-y-auto pr-2">
            {shoppingListSummaries
              .toSorted((a, b) => {
                // eslint-disable-next-line @typescript-eslint/no-magic-numbers
                if (!a.name) return -1;
                if (!b.name) return 1;
                return a.name.localeCompare(b.name);
              })
              .map(summary => {
                const { name } = summary;
                const inputValue = name ?? '';
                const selected = selectedName === name;

                return (
                  <label
                    key={inputValue || 'temporary-list'}
                    className={`flex cursor-pointer items-center gap-3 rounded-xl border p-3 transition-colors ${
                      selected
                        ? 'border-primary bg-primary-container/20'
                        : 'border-outline-variant bg-surface-container-lowest hover:bg-surface-container'
                    }`}
                  >
                    <input
                      checked={selected}
                      className="accent-primary"
                      name="shopping-list"
                      type="radio"
                      value={inputValue}
                      onChange={() => setSelectedListName(name)}
                    />
                    <span className="font-label-md text-label-md text-on-surface">
                      {listLabel(summary)}
                    </span>
                  </label>
                );
              })}
          </div>
        ) : (
          <div className="rounded-xl border border-dashed border-outline-variant bg-surface-container-low p-4 text-center">
            <span className="material-symbols-outlined text-[32px] text-primary">
              playlist_add
            </span>
            <p className="mt-2 font-label-md text-label-md text-on-surface">
              Create a shopping list to assign this item.
            </p>
          </div>
        )}

        <button
          className="flex w-full items-center justify-center gap-2 rounded-full btn-gradient px-4 py-3 font-label-md text-label-md text-on-primary-fixed-variant transition-opacity hover:opacity-90"
          type="button"
          onClick={() =>
            hasShoppingLists ? onSelectList(selectedName) : onCreateList()
          }
        >
          <span className="material-symbols-outlined icon-fill text-[20px]">
            {hasShoppingLists ? 'add_shopping_cart' : 'add'}
          </span>
          {hasShoppingLists ? 'Add to list' : 'Create shopping list'}
        </button>
      </div>
    </div>
  );
}
