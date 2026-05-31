type SearchDropdownItemProps = {
  icon: string;
  meta: string;
  muted?: boolean;
  title: string;
};

function SearchDropdownItem({
  icon,
  meta,
  muted = false,
  title,
}: Readonly<SearchDropdownItemProps>) {
  const iconClass = muted ? 'text-outline' : 'text-primary';

  return (
    <li className="flex cursor-pointer items-center gap-3 px-4 py-3 transition-colors hover:bg-surface-container">
      <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-surface-container-high">
        <span
          className={`material-symbols-outlined ${iconClass}`}
          style={{ fontVariationSettings: muted ? "'FILL' 0" : "'FILL' 1" }}
        >
          {icon}
        </span>
      </div>
      <div>
        <p className="font-label-md text-label-md text-on-surface transition-colors">
          {title}
        </p>
        <p className="font-label-sm text-label-sm text-on-surface-variant">
          {meta}
        </p>
      </div>
    </li>
  );
}

function SearchDropdown() {
  return (
    <div className="absolute left-0 top-full z-50 mt-2 hidden w-full overflow-hidden rounded-xl border border-outline-variant/30 bg-surface-container-lowest/95 shadow-lg shadow-secondary/10 backdrop-blur-md">
      <ul className="py-2">
        <SearchDropdownItem
          icon="oil_barrel"
          meta="Olio Bello - Multiple Formats"
          title="Organic Extra Virgin Olive Oil"
        />
        <SearchDropdownItem
          muted
          icon="liquor"
          meta="Generic Brand"
          title="Standard Olive Oil"
        />
      </ul>
    </div>
  );
}

function SearchInput() {
  return (
    <div className="relative w-full md:w-1/2 max-w-xl group">
      <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline">
        search
      </span>
      <input
        className="block w-full rounded-lg border border-outline-variant bg-surface-container py-2.5 pl-10 pr-10 font-body-md text-body-md text-on-surface transition-colors placeholder:text-on-surface-variant/60 focus:border-primary-container focus:ring-1 focus:ring-primary-container"
        defaultValue="Organic Extra Virgin Olive Oil"
        id="productSearch"
        placeholder="Search product to see evolution..."
        type="text"
      />
      <button
        aria-label="Clear selection"
        className="absolute right-3 top-1/2 -translate-y-1/2 text-outline transition-colors hover:text-on-surface"
        type="button"
      >
        <span className="material-symbols-outlined text-[18px]">close</span>
      </button>
      <SearchDropdown />
    </div>
  );
}

type FilterButtonProps = { icon: string; label: string };

function FilterButton({ icon, label }: Readonly<FilterButtonProps>) {
  return (
    <button
      className="flex items-center gap-2 whitespace-nowrap rounded-full border border-outline-variant bg-surface-container-lowest px-4 py-2 font-label-md text-label-md text-on-surface transition-colors hover:bg-surface-container"
      type="button"
    >
      <span className="material-symbols-outlined text-[18px]">{icon}</span>
      {label}
      <span className="material-symbols-outlined text-[16px]">
        arrow_drop_down
      </span>
    </button>
  );
}

export default function EvolutionPageHeader() {
  return (
    <header className="sticky top-16 z-30 flex flex-col items-start justify-between gap-4 border-b border-surface-variant bg-surface/90 px-container-margin py-stack-md shadow-sm shadow-secondary/5 backdrop-blur-md md:flex-row md:items-center">
      <SearchInput />
      <div className="flex w-full items-center gap-3 overflow-x-auto pb-1 md:w-auto md:pb-0">
        <FilterButton icon="storefront" label="Market Filter" />
        <FilterButton icon="sell" label="Brand Filter" />
      </div>
    </header>
  );
}
