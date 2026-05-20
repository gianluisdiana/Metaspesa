import { ShoppingItemBadgeViewModel } from '@/lib/shopping-list';

const BADGE_TONE_CLASSES: Record<ShoppingItemBadgeViewModel['tone'], string> = {
  item: 'bg-tertiary-container/30 text-on-tertiary-container',
};

export function ItemBadge({
  label,
  tone,
}: Readonly<ShoppingItemBadgeViewModel>) {
  return (
    <span className={`px-2 py-0.5 rounded-md ${BADGE_TONE_CLASSES[tone]}`}>
      {label}
    </span>
  );
}
