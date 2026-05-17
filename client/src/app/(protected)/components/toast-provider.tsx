'use client';

import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react';

type ToastTone = 'error' | 'info' | 'success';

type Toast = {
  id: number;
  message: string;
  tone: ToastTone;
};

type ToastInput = {
  message: string;
  tone?: ToastTone;
};

type ToastContextValue = {
  showToast: (toast: ToastInput) => void;
};

const TOAST_TIMEOUT_MS = 4_000;
const ToastContext = createContext<ToastContextValue | null>(null);

const TONE_STYLES: Record<
  ToastTone,
  { container: string; icon: string; iconName: string }
> = {
  error: {
    container: 'bg-error-container text-on-error-container',
    icon: 'text-error',
    iconName: 'error',
  },
  info: {
    container: 'bg-inverse-surface text-inverse-on-surface',
    icon: 'text-tertiary-fixed-dim',
    iconName: 'info',
  },
  success: {
    container: 'bg-inverse-surface text-inverse-on-surface',
    icon: 'text-tertiary-fixed-dim',
    iconName: 'check_circle',
  },
};

function ToastItem({
  toast,
  onDismiss,
}: Readonly<{
  toast: Toast;
  onDismiss: (id: number) => void;
}>) {
  const toneStyle = TONE_STYLES[toast.tone];

  return (
    <div
      className={`pointer-events-auto flex max-w-sm items-center gap-3 rounded-full px-5 py-3 shadow-xl ${toneStyle.container}`}
      role={toast.tone === 'error' ? 'alert' : 'status'}
    >
      <span
        className={`material-symbols-outlined icon-fill text-[22px] ${toneStyle.icon}`}
      >
        {toneStyle.iconName}
      </span>
      <span className="font-label-md text-label-md">{toast.message}</span>
      <button
        aria-label="Dismiss notification"
        className="ml-auto flex size-7 items-center justify-center rounded-full opacity-70 transition-opacity hover:opacity-100"
        type="button"
        onClick={() => onDismiss(toast.id)}
      >
        <span className="material-symbols-outlined text-[18px]">close</span>
      </button>
    </div>
  );
}

function ToastViewport({
  onDismiss,
  toasts,
}: Readonly<{
  onDismiss: (id: number) => void;
  toasts: Toast[];
}>) {
  return (
    <div className="pointer-events-none fixed bottom-24 left-1/2 z-110 flex w-[calc(100%-2rem)] -translate-x-1/2 flex-col items-center gap-3 md:bottom-8 md:left-auto md:right-8 md:w-auto md:translate-x-0 md:items-end">
      {toasts.map(toast => (
        <ToastItem key={toast.id} toast={toast} onDismiss={onDismiss} />
      ))}
    </div>
  );
}

export function ToastProvider({ children }: Readonly<{ children: ReactNode }>) {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const dismissToast = useCallback((id: number) => {
    setToasts(currentToasts =>
      currentToasts.filter(currentToast => currentToast.id !== id),
    );
  }, []);

  const showToast = useCallback(
    ({ message, tone = 'info' }: ToastInput) => {
      const id = Date.now();
      setToasts(currentToasts => [
        ...currentToasts,
        {
          id,
          message,
          tone,
        },
      ]);
      globalThis.setTimeout(() => dismissToast(id), TOAST_TIMEOUT_MS);
    },
    [dismissToast],
  );

  const contextValue = useMemo(() => ({ showToast }), [showToast]);

  return (
    <ToastContext value={contextValue}>
      {children}
      <ToastViewport toasts={toasts} onDismiss={dismissToast} />
    </ToastContext>
  );
}

export function useToast() {
  const contextValue = useContext(ToastContext);
  if (!contextValue) {
    throw new Error('useToast must be used inside ToastProvider.');
  }

  return contextValue;
}
