import LoginCard from './login-card';

export function MobileHeader() {
  return (
    <div className="md:hidden text-center mb-stack-lg">
      <h1 className="font-headline-lg text-headline-lg text-primary tracking-tight">
        Metaspesa
      </h1>
      <p className="font-body-md text-body-md text-on-surface-variant mt-stack-sm">
        Welcome back to your calm kitchen.
      </p>
    </div>
  );
}

export function HelperText() {
  return (
    <div className="mt-stack-lg text-center opacity-70">
      <p className="font-label-sm text-label-sm text-on-surface-variant">
        Secure, calm, and ready to list.
      </p>
    </div>
  );
}

export default function RightLoginPanel() {
  return (
    <div className="flex-1 flex items-center justify-center p-container-margin md:p-stack-lg bg-surface relative">
      <div className="absolute top-0 right-0 w-64 h-64 bg-primary-fixed/20 rounded-full blur-3xl -translate-y-1/2 translate-x-1/4 pointer-events-none" />
      <div className="absolute bottom-0 left-0 w-96 h-96 bg-secondary-fixed/20 rounded-full blur-3xl translate-y-1/4 -translate-x-1/4 pointer-events-none" />
      <div className="w-full max-w-sm relative z-10">
        <MobileHeader />
        <LoginCard />
        <HelperText />
      </div>
    </div>
  );
}
