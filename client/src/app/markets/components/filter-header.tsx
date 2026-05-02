export function MarketTitle() {
  return (
    <div className="flex items-center justify-between">
      <h1 className="font-headline-lg text-headline-lg text-on-surface">
        Whole Foods Market
      </h1>
    </div>
  );
}

export function SearchBar() {
  return (
    <div className="relative flex-grow md:max-w-md">
      <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline">
        search
      </span>
      <input
        className="w-full bg-surface-container-highest border-none rounded-full py-2 pl-10 pr-4 text-body-md font-body-md text-on-surface placeholder:text-on-surface-variant focus:ring-2 focus:ring-primary focus:bg-surface-container-lowest transition-all shadow-inner"
        placeholder="Search products..."
        type="text"
      />
    </div>
  );
}

export function FilterDropdown({ label }: Readonly<{ label: string }>) {
  return (
    <div className="relative group">
      <button className="flex items-center gap-2 bg-surface-container border border-outline-variant rounded-full px-4 py-2 font-label-md text-label-md text-on-surface hover:bg-surface-container-high transition-colors">
        {label}
        <span className="material-symbols-outlined text-[18px]">
          expand_more
        </span>
      </button>
    </div>
  );
}

export default function FilterHeader() {
  return (
    <div className="sticky top-[64px] z-30 bg-surface/90 backdrop-blur-md border-b border-surface-variant px-container-margin py-stack-md flex flex-col gap-stack-sm shadow-sm shadow-secondary/5">
      <MarketTitle />
      <div className="flex flex-wrap gap-unit mt-unit items-center">
        <SearchBar />
        <FilterDropdown label="Category" />
        <FilterDropdown label="Brand" />
      </div>
    </div>
  );
}
