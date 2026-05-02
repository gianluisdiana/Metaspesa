function SearchFilter() {
  return (
    <div className="flex-1 w-full flex flex-col gap-unit">
      <label className="font-label-sm text-label-sm text-on-surface-variant">
        Search Product
      </label>
      <div className="relative">
        <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline">
          search
        </span>
        <input
          className="w-full bg-surface-container-low border-none rounded-lg pl-10 pr-4 py-3 font-body-md text-body-md text-on-surface focus:ring-2 focus:ring-tertiary-container transition-all"
          placeholder="e.g., Organic Avocados..."
          type="text"
        />
      </div>
    </div>
  );
}

function MarketFilter() {
  return (
    <div className="w-full md:w-48 flex flex-col gap-unit">
      <label className="font-label-sm text-label-sm text-on-surface-variant">
        Market
      </label>
      <div className="relative">
        <select className="w-full bg-surface-container-low border-none rounded-lg pl-4 pr-10 py-3 font-body-md text-body-md text-on-surface appearance-none focus:ring-2 focus:ring-tertiary-container transition-all">
          <option>All Markets</option>
          <option>Whole Foods</option>
          <option>{"Trader Joe's"}</option>
        </select>
        <span className="material-symbols-outlined absolute right-3 top-1/2 -translate-y-1/2 text-outline pointer-events-none">
          expand_more
        </span>
      </div>
    </div>
  );
}

type TimePeriodProps = { active?: boolean; label: string };

function TimePeriod({ active = false, label }: Readonly<TimePeriodProps>) {
  const activeClass = 'bg-white text-primary shadow-sm';
  const inactiveClass =
    'text-on-surface-variant hover:bg-surface-container-high transition-colors';
  return (
    <button
      className={`px-4 py-2 rounded-md font-label-md text-label-md ${active ? activeClass : inactiveClass}`}
    >
      {label}
    </button>
  );
}

function TimeFilter() {
  return (
    <div className="w-full md:w-auto flex flex-col gap-unit">
      <label className="font-label-sm text-label-sm text-on-surface-variant opacity-0 hidden md:block">
        Time
      </label>
      <div className="flex bg-surface-container-low rounded-lg p-1">
        <TimePeriod label="1M" />
        <TimePeriod active label="6M" />
        <TimePeriod label="1Y" />
      </div>
    </div>
  );
}

export default function FiltersCard() {
  return (
    <div className="bg-surface-container-lowest/80 backdrop-blur-md border border-outline-variant/30 rounded-xl p-6 shadow-[0_4px_24px_rgba(168,85,247,0.04)] mb-stack-lg flex flex-col md:flex-row gap-gutter items-end">
      <SearchFilter />
      <MarketFilter />
      <TimeFilter />
    </div>
  );
}
