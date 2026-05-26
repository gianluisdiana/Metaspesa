'use client';

import { FormEvent, useState } from 'react';

export function TemporaryListNameModal({
  isSaving,
  message,
  onCancel,
  onConfirm,
}: Readonly<{
  isSaving: boolean;
  message: string;
  onCancel: () => void;
  onConfirm: (name: string) => void;
}>) {
  const [name, setName] = useState('');
  const trimmedName = name.trim();

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!trimmedName || isSaving) {
      return;
    }

    onConfirm(trimmedName);
  }

  return (
    <div
      className="fixed inset-0 z-100 flex items-center justify-center bg-on-surface/40 p-4 backdrop-blur-sm"
      role="presentation"
      onClick={isSaving ? undefined : onCancel}
    >
      <form
        aria-modal="true"
        className="flex w-full max-w-sm flex-col gap-5 rounded-2xl bg-surface-container-lowest p-6 shadow-2xl"
        role="dialog"
        onClick={event => event.stopPropagation()}
        onSubmit={handleSubmit}
      >
        <div className="flex items-start gap-3">
          <span className="material-symbols-outlined text-primary">
            edit_note
          </span>
          <div className="flex flex-col gap-1">
            <h3 className="font-headline-md text-headline-md text-on-surface">
              Name temporary list
            </h3>
            <p className="font-body-md text-body-md text-on-surface-variant">
              {message}
            </p>
          </div>
        </div>

        <label className="flex flex-col gap-2">
          <span className="font-label-md text-label-md text-on-surface">
            List name
          </span>
          <input
            autoFocus
            className="w-full rounded-lg bg-surface-container px-4 py-3 font-body-md text-body-md text-on-surface outline-none transition-colors placeholder:text-on-surface-variant/50 focus:bg-surface-container-lowest focus:ring-2 focus:ring-tertiary-container"
            disabled={isSaving}
            placeholder="Groceries"
            type="text"
            value={name}
            onChange={event => setName(event.target.value)}
          />
        </label>

        <div className="flex justify-end gap-3">
          <button
            className="rounded-full px-4 py-2 font-label-md text-label-md text-on-surface-variant transition-colors hover:bg-surface-container disabled:opacity-60"
            disabled={isSaving}
            type="button"
            onClick={onCancel}
          >
            Cancel
          </button>
          <button
            className="rounded-full bg-primary px-4 py-2 font-label-md text-label-md text-on-primary transition-opacity hover:opacity-90 disabled:opacity-60"
            disabled={!trimmedName || isSaving}
            type="submit"
          >
            {isSaving ? 'Creating...' : 'Create'}
          </button>
        </div>
      </form>
    </div>
  );
}
