export function CategoryHeader({
  first = false,
  label,
}: Readonly<{ first?: boolean; label: string }>) {
  return (
    <h3
      className={`font-label-md text-label-md text-on-surface-variant ${first ? 'mt-2' : 'mt-4'} mb-1 px-2 uppercase tracking-wider`}
    >
      {label}
    </h3>
  );
}

export function CompletedDivider({ count }: Readonly<{ count: number }>) {
  return (
    <div className="flex items-center gap-4 mt-6 mb-2 opacity-60">
      <div className="h-px bg-outline-variant flex-1" />
      <span className="font-label-sm text-label-sm text-on-surface-variant uppercase tracking-widest">
        Completed ({count})
      </span>
      <div className="h-px bg-outline-variant flex-1" />
    </div>
  );
}

export function EmptyList() {
  return (
    <div className="bg-surface-container-lowest border border-dashed border-outline-variant rounded-2xl p-stack-lg text-center mt-stack-md">
      <p className="font-body-lg text-body-lg text-on-surface">No items yet</p>
    </div>
  );
}
