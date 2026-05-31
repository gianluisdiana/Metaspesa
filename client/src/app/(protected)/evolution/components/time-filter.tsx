type TimeFilterOptionProps = {
  active?: boolean;
  label: string;
};

function TimeFilterOption({
  active = false,
  label,
}: Readonly<TimeFilterOptionProps>) {
  const activeClass =
    'bg-surface-container-lowest text-primary shadow-sm border border-outline-variant/30';
  const inactiveClass =
    'text-on-surface-variant hover:bg-surface-container-lowest hover:text-on-surface';

  return (
    <button
      className={`rounded-md px-4 py-1.5 font-label-md text-label-md transition-colors ${active ? activeClass : inactiveClass}`}
      type="button"
    >
      {label}
    </button>
  );
}

export default function TimeFilter() {
  return (
    <div className="flex self-start rounded-lg border border-outline-variant/50 bg-surface-container p-1 sm:self-auto">
      <TimeFilterOption label="1M" />
      <TimeFilterOption active label="3M" />
      <TimeFilterOption label="1Y" />
    </div>
  );
}
