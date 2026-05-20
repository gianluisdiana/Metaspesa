import {
  CheckedShoppingItemViewModel,
  UncheckedShoppingItemViewModel,
} from '@/lib/shopping-list';

import { ItemBadge } from './shopping-list-badge';

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

export function UncheckedItem({
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

export function CheckedItem({
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
