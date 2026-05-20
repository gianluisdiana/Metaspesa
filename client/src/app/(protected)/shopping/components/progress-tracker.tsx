import { ShoppingProgressViewModel } from '@/lib/shopping-list';

export function ProgressTracker({
  progress,
}: Readonly<{ progress: ShoppingProgressViewModel }>) {
  return (
    <div className="bg-surface-container-lowest p-stack-md rounded-2xl shadow-[0_4px_20px_rgba(208,197,253,0.1)] border border-surface-container-highest flex flex-col gap-stack-sm">
      <div className="flex justify-between items-center font-label-sm text-label-sm text-on-surface-variant">
        <span>Shopping Progress</span>
        <span className="font-bold text-primary">{progress.label}</span>
      </div>
      <div className="h-2 w-full bg-surface-variant rounded-full overflow-hidden">
        <div
          className="h-full bg-linear-to-r from-tertiary to-primary rounded-full transition-all duration-500 ease-out"
          style={{ width: `${progress.percentage}%` }}
        />
      </div>
    </div>
  );
}
