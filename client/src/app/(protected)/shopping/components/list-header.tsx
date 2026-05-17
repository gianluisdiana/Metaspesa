import { ShoppingListTabViewModel } from '@/lib/shopping-list-view-model';

type ListPageHeaderProps = {
  itemCountLabel: string;
  listName: string;
};

export function ListPageHeader({
  itemCountLabel,
  listName,
}: Readonly<ListPageHeaderProps>) {
  return (
    <div className="flex justify-between items-end">
      <div>
        <h2 className="font-headline-lg text-headline-lg text-on-surface tracking-tight">
          {listName}
        </h2>
        <p className="font-body-md text-body-md text-on-surface-variant mt-1">
          {itemCountLabel}
        </p>
      </div>
      <button
        aria-label="Share the list with others"
        className="flex items-center gap-2 bg-secondary-container/40 text-on-secondary-container hover:bg-secondary-container/60 transition-colors px-4 py-2 rounded-full font-label-md text-label-md"
        type="button"
      >
        <span
          className="material-symbols-outlined text-[18px]"
          style={{ fontVariationSettings: "'FILL' 0" }}
        >
          share
        </span>
        {''}
        Share
      </button>
    </div>
  );
}

type ListTabProps = {
  tab: ShoppingListTabViewModel;
  onSelectList: (name?: string) => void;
};

function ListTab({ onSelectList, tab }: Readonly<ListTabProps>) {
  const activeClass =
    'bg-primary text-on-primary shadow-[0_2px_8px_rgba(135,82,0,0.2)] border border-transparent';
  const inactiveClass =
    'bg-surface-container-lowest text-on-surface-variant border border-outline-variant hover:bg-surface-container transition-colors';
  return (
    <button
      className={`whitespace-nowrap px-5 py-2 rounded-full font-label-md text-label-md ${tab.active ? activeClass : inactiveClass}`}
      onClick={() => onSelectList(tab.name)}
      type="button"
    >
      {tab.label}
    </button>
  );
}

export default function ListTabs({
  isCreating,
  onCreateList,
  onSelectList,
  tabs,
}: Readonly<{
  isCreating: boolean;
  onCreateList: () => void;
  onSelectList: (name?: string) => void;
  tabs: ShoppingListTabViewModel[];
}>) {
  return (
    <div className="flex gap-3 overflow-x-auto pb-2 -mx-2 px-2 scrollbar-hide">
      {tabs.map(tab => (
        <ListTab
          key={tab.name ?? 'temporary'}
          onSelectList={onSelectList}
          tab={tab}
        />
      ))}
      <button
        aria-label="Placeholder for creating another shopping list"
        className="whitespace-nowrap w-10 h-10 flex items-center justify-center rounded-full bg-surface-container text-on-surface-variant border border-dashed border-outline hover:bg-surface-container-high transition-colors"
        disabled={isCreating}
        onClick={onCreateList}
        type="button"
      >
        <span
          className="material-symbols-outlined"
          style={{ fontVariationSettings: "'FILL' 0" }}
        >
          {isCreating ? 'hourglass_top' : 'add'}
        </span>
      </button>
    </div>
  );
}
