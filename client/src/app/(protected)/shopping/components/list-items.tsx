import { Fragment } from 'react';

import {
  CheckedShoppingItemViewModel,
  ShoppingItemSectionViewModel,
  ShoppingProgressViewModel,
  UncheckedShoppingItemViewModel,
} from '@/lib/shopping-list-view-model';

function ItemBadge({
  colorClass,
  label,
}: Readonly<{ colorClass: string; label: string }>) {
  return (
    <span className={`px-2 py-0.5 rounded-md ${colorClass}`}>{label}</span>
  );
}

function LowStockWarning() {
  return (
    <span className="text-error flex items-center text-[10px]">
      <span className="material-symbols-outlined text-[14px] mr-1">
        warning
      </span>
      {''}
      Low Stock
    </span>
  );
}

function CategoryHeader({
  first = false,
  label,
}: Readonly<{ first?: boolean; label: string }>) {
  return (
    <h3
      className={`font-label-md text-label-md text-on-surface-variant ${first ? 'mt-2' : 'mt-4'} mb-1 px-2 uppercase tracking-wider`}
    >
      {label}
    </h3>
  );
}

function CompletedDivider({ count }: Readonly<{ count: number }>) {
  return (
    <div className="flex items-center gap-4 mt-6 mb-2 opacity-60">
      <div className="h-px bg-outline-variant flex-1" />
      <span className="font-label-sm text-label-sm text-on-surface-variant uppercase tracking-widest">
        Completed ({count})
      </span>
      <div className="h-px bg-outline-variant flex-1" />
    </div>
  );
}

function UncheckedItem({
  item,
}: Readonly<{ item: UncheckedShoppingItemViewModel }>) {
  return (
    <div className="group flex items-center gap-stack-md bg-surface-container-lowest p-stack-md rounded-xl shadow-[0_2px_8px_rgba(208,197,253,0.08)] border border-transparent hover:border-primary-container/30 transition-all">
      <button className="w-7 h-7 rounded-full border-[2.5px] border-primary shrink-0 hover:bg-primary/10 transition-colors" />
      <div className="flex-1 flex flex-col sm:flex-row sm:items-center justify-between gap-2">
        <div className="flex flex-col">
          <span className="font-body-lg text-body-lg text-on-surface">
            {item.name}
          </span>
          <span className="font-label-sm text-label-sm text-on-surface-variant flex items-center gap-2 mt-0.5">
            <ItemBadge {...item.badge} />
            {item.lowStock && <LowStockWarning />}
          </span>
        </div>
        <div className="flex items-center gap-stack-lg">
          <div className="flex flex-col items-end">
            <span className="font-label-md text-label-md text-on-surface">
              {item.price}
            </span>
            <span className="font-label-sm text-label-sm text-on-surface-variant">
              Qty: {item.qty}
            </span>
          </div>
          <button
            aria-label={`Remove ${item.name} placeholder`}
            className="text-outline hover:text-error transition-colors opacity-0 group-hover:opacity-100 p-2"
            type="button"
          >
            <span
              className="material-symbols-outlined text-[20px]"
              style={{ fontVariationSettings: "'FILL' 0" }}
            >
              delete
            </span>
          </button>
        </div>
      </div>
    </div>
  );
}

function CheckedItem({
  item,
}: Readonly<{ item: CheckedShoppingItemViewModel }>) {
  return (
    <div className="flex items-center gap-stack-md bg-secondary-fixed/30 p-stack-md rounded-xl border border-secondary-fixed/50 transition-all opacity-80">
      <button
        aria-label={`${item.name} checked placeholder`}
        className="w-7 h-7 rounded-full bg-primary border-primary shrink-0 flex items-center justify-center text-on-primary"
        type="button"
      >
        <span
          className="material-symbols-outlined text-[18px]"
          style={{ fontVariationSettings: "'wght' 700" }}
        >
          check
        </span>
      </button>
      <div className="flex-1 flex justify-between items-center">
        <span className="font-body-lg text-body-lg text-on-surface-variant line-through decoration-outline-variant">
          {item.name}
        </span>
        <div className="font-label-md text-label-md text-on-surface-variant line-through decoration-outline-variant">
          {item.price}
        </div>
      </div>
    </div>
  );
}

export function ProgressTracker({
  progress,
}: Readonly<{ progress: ShoppingProgressViewModel }>) {
  return (
    <div className="bg-surface-container-lowest p-stack-md rounded-2xl shadow-[0_4px_20px_rgba(208,197,253,0.1)] border border-surface-container-highest flex flex-col gap-stack-sm">
      <div className="flex justify-between items-center font-label-sm text-label-sm text-on-surface-variant">
        <span>Shopping Progress</span>
        <span className="font-bold text-primary">{progress.label}</span>
      </div>
      <div className="h-2 w-full bg-surface-variant rounded-full overflow-hidden">
        <div
          className="h-full bg-linear-to-r from-tertiary to-primary rounded-full transition-all duration-500 ease-out"
          style={{ width: `${progress.percentage}%` }}
        />
      </div>
    </div>
  );
}

function EmptyList() {
  return (
    <div className="bg-surface-container-lowest border border-dashed border-outline-variant rounded-2xl p-stack-lg text-center mt-stack-md">
      <p className="font-body-lg text-body-lg text-on-surface">No items yet</p>
    </div>
  );
}

export default function ItemsContainer({
  checkedItems,
  hasItems,
  uncheckedSections,
}: Readonly<{
  checkedItems: CheckedShoppingItemViewModel[];
  hasItems: boolean;
  uncheckedSections: ShoppingItemSectionViewModel[];
}>) {
  if (!hasItems) {
    return <EmptyList />;
  }

  return (
    <div className="flex flex-col gap-unit">
      {uncheckedSections.map((section, idx) => (
        <Fragment key={section.label}>
          <CategoryHeader first={idx === 0} label={section.label} />
          {section.items.map(item => (
            <UncheckedItem key={item.id} item={item} />
          ))}
        </Fragment>
      ))}
      {checkedItems.length > 0 && (
        <>
          <CompletedDivider count={checkedItems.length} />
          {checkedItems.map(item => (
            <CheckedItem key={item.id} item={item} />
          ))}
        </>
      )}
    </div>
  );
}
