const TABS = ['Weekly Grocery', 'Office Supplies', 'Party Prep'];

export function ListPageHeader() {
  return (
    <div className="flex justify-between items-end">
      <div>
        <h2 className="font-headline-lg text-headline-lg text-on-surface tracking-tight">
          Weekly Grocery
        </h2>
        <p className="font-body-md text-body-md text-on-surface-variant mt-1">
          {"Trader Joe's • 12 Items"}
        </p>
      </div>
      <button className="flex items-center gap-2 bg-secondary-container/40 text-on-secondary-container hover:bg-secondary-container/60 transition-colors px-4 py-2 rounded-full font-label-md text-label-md">
        <span
          className="material-symbols-outlined text-[18px]"
          style={{ fontVariationSettings: "'FILL' 0" }}
        >
          share
        </span>
        Share
      </button>
    </div>
  );
}

type ListTabProps = { active?: boolean; label: string };

function ListTab({ active = false, label }: Readonly<ListTabProps>) {
  const activeClass =
    'bg-primary text-on-primary shadow-[0_2px_8px_rgba(135,82,0,0.2)] border border-transparent';
  const inactiveClass =
    'bg-surface-container-lowest text-on-surface-variant border border-outline-variant hover:bg-surface-container transition-colors';
  return (
    <button
      className={`whitespace-nowrap px-5 py-2 rounded-full font-label-md text-label-md ${active ? activeClass : inactiveClass}`}
    >
      {label}
    </button>
  );
}

export default function ListTabs() {
  return (
    <div className="flex gap-3 overflow-x-auto pb-2 -mx-2 px-2 scrollbar-hide">
      {TABS.map((tab, i) => (
        <ListTab key={tab} active={i === 0} label={tab} />
      ))}
      <button className="whitespace-nowrap w-10 h-10 flex items-center justify-center rounded-full bg-surface-container text-on-surface-variant border border-dashed border-outline hover:bg-surface-container-high transition-colors">
        <span
          className="material-symbols-outlined"
          style={{ fontVariationSettings: "'FILL' 0" }}
        >
          add
        </span>
      </button>
    </div>
  );
}
