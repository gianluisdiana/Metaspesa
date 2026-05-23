'use client';

export function DeleteItemConfirmationModal({
  itemName,
  onCancel,
  onConfirm,
}: Readonly<{
  itemName: string;
  onCancel: () => void;
  onConfirm: () => void;
}>) {
  return (
    <div
      className="fixed inset-0 z-[100] flex items-center justify-center bg-on-surface/40 p-4 backdrop-blur-sm"
      role="presentation"
      onClick={onCancel}
    >
      <div
        aria-modal="true"
        className="flex w-full max-w-sm flex-col gap-5 rounded-2xl bg-surface-container-lowest p-6 shadow-2xl"
        role="dialog"
        onClick={event => event.stopPropagation()}
      >
        <div className="flex items-start gap-3">
          <span className="material-symbols-outlined text-error">delete</span>
          <div className="flex flex-col gap-1">
            <h3 className="font-headline-md text-headline-md text-on-surface">
              Delete item
            </h3>
            <p className="font-body-md text-body-md text-on-surface-variant">
              Remove {itemName} from this shopping list?
            </p>
          </div>
        </div>
        <div className="flex justify-end gap-3">
          <button
            className="rounded-full px-4 py-2 font-label-md text-label-md text-on-surface-variant transition-colors hover:bg-surface-container"
            type="button"
            onClick={onCancel}
          >
            Cancel
          </button>
          <button
            className="rounded-full bg-error px-4 py-2 font-label-md text-label-md text-on-error transition-opacity hover:opacity-90"
            type="button"
            onClick={onConfirm}
          >
            Delete
          </button>
        </div>
      </div>
    </div>
  );
}
