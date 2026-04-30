import TextField from '@/app/components/text-field';

import DecorativeHeader from './decorative-header';

export function FormHeading() {
  return (
    <div className="text-center mb-8">
      <h1 className="font-headline-lg text-headline-lg text-on-surface mb-2">
        Join Metaspesa
      </h1>
      <p className="font-body-md text-body-md text-on-surface-variant">
        Your grocery assistant awaits.
      </p>
    </div>
  );
}

export function RegisterButton() {
  return (
    <div className="pt-4">
      <button
        className="w-full bg-gradient-to-r from-primary-fixed-dim to-primary-container text-on-primary-container font-label-md text-label-md py-3 px-6 rounded-full shadow-sm hover:shadow-md transition-all active:scale-[0.98] relative overflow-hidden group"
        type="button"
      >
        <div className="absolute top-0 left-0 w-full h-[1px] bg-white/40" />
        <span className="relative z-10 flex items-center justify-center gap-2">
          Create Account
          <span className="material-symbols-outlined text-sm">
            arrow_forward
          </span>
        </span>
      </button>
    </div>
  );
}

export function LoginLink() {
  return (
    <div className="mt-8 text-center">
      <p className="font-body-md text-body-md text-on-surface-variant">
        Already have an account?{' '}
        <a
          className="font-label-md text-label-md text-primary hover:text-primary-container transition-colors ml-1"
          href="/auth/login"
        >
          Log in here
        </a>
      </p>
    </div>
  );
}

export default function RegisterCard() {
  return (
    <div className="w-full max-w-md bg-surface-container-lowest rounded-xl shadow-[0_8px_32px_rgba(168,85,247,0.05)] overflow-hidden relative">
      <DecorativeHeader />
      <div className="p-8">
        <FormHeading />
        <form action="#" className="space-y-stack-lg">
          <TextField
            id="username"
            label="Username"
            icon="person"
            placeholder="e.g. shopper"
          />
          <TextField
            id="password"
            label="Password"
            icon="lock"
            type="password"
            placeholder="••••••••"
          />
          <TextField
            id="confirm-password"
            label="Confirm Password"
            icon="lock_clock"
            type="password"
            placeholder="••••••••"
          />
          <RegisterButton />
        </form>
        <LoginLink />
      </div>
    </div>
  );
}
