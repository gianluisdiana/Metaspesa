export function NoShoppingLists({
  isCreating,
  onCreateList,
}: Readonly<{
  isCreating: boolean;
  onCreateList: () => void;
}>) {
  return (
    <div className="min-h-[calc(100vh-8rem)] px-container-margin py-stack-lg flex items-center justify-center">
      <div className="w-full max-w-xl bg-surface-container-lowest border border-outline-variant rounded-3xl p-stack-lg sm:p-8 text-center shadow-sm">
        <div className="mx-auto mb-stack-md w-16 h-16 rounded-full bg-primary-container text-on-primary-container flex items-center justify-center">
          <span
            aria-hidden="true"
            className="material-symbols-outlined text-[32px]"
            style={{ fontVariationSettings: "'FILL' 0" }}
          >
            receipt_long
          </span>
        </div>
        <h1 className="font-headline-lg text-headline-lg text-on-surface tracking-tight">
          No shopping lists yet
        </h1>
        <p className="font-body-lg text-body-lg text-on-surface-variant mt-stack-sm">
          Create your first list to organize what you need and keep track of
          prices while you shop.
        </p>
        <button
          className="mt-stack-lg inline-flex items-center justify-center gap-2 rounded-full bg-primary px-6 py-3 font-label-lg text-label-lg text-on-primary shadow-sm transition-colors hover:bg-primary/90 disabled:cursor-wait disabled:opacity-70"
          disabled={isCreating}
          onClick={onCreateList}
          type="button"
        >
          <span
            aria-hidden="true"
            className="material-symbols-outlined text-[20px]"
            style={{ fontVariationSettings: "'FILL' 0" }}
          >
            {isCreating ? 'hourglass_top' : 'add'}
          </span>
          {isCreating ? 'Creating...' : 'Create shopping list'}
        </button>
      </div>
    </div>
  );
}
