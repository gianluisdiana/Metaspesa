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
