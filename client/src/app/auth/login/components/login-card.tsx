import TextField from '@/app/components/text-field';

export function CardHeading() {
  return (
    <div className="mb-stack-lg">
      <h2 className="font-headline-md text-headline-md text-on-surface">
        Sign In
      </h2>
      <p className="font-body-md text-body-md text-on-surface-variant mt-unit">
        Please enter your credentials to continue.
      </p>
    </div>
  );
}

export function LoginButton() {
  return (
    <div className="pt-stack-sm">
      <button
        className="w-full relative overflow-hidden bg-linear-to-r from-primary-fixed to-primary-container text-on-primary-container font-label-md text-label-md py-3 px-4 rounded-lg flex items-center justify-center gap-2 shadow-sm shadow-primary/20 hover:shadow-md hover:-translate-y-0.5 transition-all duration-200 group"
        type="submit"
      >
        <div className="absolute inset-0 inner-glow rounded-lg pointer-events-none" />
        <span>Login</span>
        <span className="material-symbols-outlined text-[18px] group-hover:translate-x-1 transition-transform">
          arrow_forward
        </span>
      </button>
    </div>
  );
}

export function RegistrationLink() {
  return (
    <div className="mt-stack-lg text-center">
      <p className="font-body-md text-body-md text-on-surface-variant">
        New here?{' '}
        <a
          className="font-label-md text-label-md text-primary hover:text-primary-fixed-dim transition-colors underline-offset-4 hover:underline"
          href="/auth/register"
        >
          Create an account
        </a>
      </p>
    </div>
  );
}

export default function LoginCard() {
  return (
    <div className="bg-surface-container-lowest/70 backdrop-blur-xl border border-surface-variant rounded-xl p-stack-lg md:p-[32px] shadow-[0_8px_32px_rgba(97,88,136,0.08)]">
      <CardHeading />
      <form action="#" className="space-y-stack-md">
        <TextField
          id="username"
          label="Username"
          icon="person"
          placeholder="Enter your username"
        />
        <TextField
          id="password"
          label="Password"
          icon="lock"
          type="password"
          placeholder="••••••••"
        />
        <LoginButton />
      </form>
      <RegistrationLink />
    </div>
  );
}
