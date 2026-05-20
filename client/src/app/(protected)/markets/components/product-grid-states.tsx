export function EmptyState() {
  return (
    <div className="flex flex-col items-center justify-center p-container-margin text-on-surface-variant">
      <span className="material-symbols-outlined text-[48px]">search_off</span>
      <p className="font-body-lg text-body-lg mt-2">No products found</p>
    </div>
  );
}

export function LoadingState() {
  return (
    <div className="flex justify-center py-stack-lg text-on-surface-variant">
      <span className="material-symbols-outlined animate-spin text-[28px]">
        progress_activity
      </span>
    </div>
  );
}

export function RetryButton({ onRetry }: Readonly<{ onRetry: () => void }>) {
  return (
    <button
      className="mx-auto rounded-full bg-surface-container px-4 py-2 font-label-md text-label-md text-on-surface hover:bg-surface-container-high"
      type="button"
      onClick={onRetry}
    >
      Retry
    </button>
  );
}
