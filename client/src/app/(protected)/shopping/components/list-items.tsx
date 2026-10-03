import { Fragment } from 'react';

import {
  CheckedShoppingItemViewModel,
  ShoppingItemSectionViewModel,
} from '@/lib/shopping-list';

import { CheckedItem, UncheckedItem } from './shopping-list-item-row';
import {
  CategoryHeader,
  CompletedDivider,
  EmptyList,
} from './shopping-list-sections';

export default function ItemsContainer({
  checkedItems,
  hasItems,
  onRequestDeleteItem,
  onToggleItemChecked,
  uncheckedSections,
}: Readonly<{
  checkedItems: CheckedShoppingItemViewModel[];
  hasItems: boolean;
  onRequestDeleteItem: (shoppingItemId: string, itemName: string) => void;
  onToggleItemChecked: (shoppingItemId: string, checked: boolean) => void;
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
            <UncheckedItem
              key={item.id}
              item={item}
              onDelete={() =>
                onRequestDeleteItem(item.shoppingItemId, item.name)
              }
              onToggleChecked={() =>
                onToggleItemChecked(item.shoppingItemId, true)
              }
            />
          ))}
        </Fragment>
      ))}
      {checkedItems.length > 0 && (
        <>
          <CompletedDivider count={checkedItems.length} />
          {checkedItems.map(item => (
            <CheckedItem
              key={item.id}
              item={item}
              onToggleChecked={() =>
                onToggleItemChecked(item.shoppingItemId, false)
              }
            />
          ))}
        </>
      )}
    </div>
  );
}
