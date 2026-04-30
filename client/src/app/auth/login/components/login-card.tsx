export function CardHeading() {
  return (
    <div className="mb-stack-lg">
      <h2 className="font-headline-md text-headline-md text-on-surface">
        Sign In
      </h2>
      <p className="font-body-md text-body-md text-on-surface-variant mt-unit">
        Please enter your username to continue.
      </p>
    </div>
  );
}

export function UsernameField() {
  return (
    <div className="space-y-unit">
      <label
        className="block font-label-md text-label-md text-on-surface"
        htmlFor="username"
      >
        Username
      </label>
      <div className="relative">
        <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <span
            className="material-symbols-outlined text-on-surface-variant"
            style={{ fontSize: '20px' }}
          >
            person
          </span>
        </div>
        <input
          className="block w-full pl-10 pr-3 py-3 bg-surface-container border-transparent rounded-lg font-body-md text-body-md text-on-surface placeholder:text-outline focus:border-transparent focus:ring-2 focus:ring-tertiary-fixed-dim focus:bg-surface-container-highest transition-all duration-200"
          id="username"
          name="username"
          placeholder="Enter your username"
          type="text"
        />
      </div>
    </div>
  );
}

export function LoginButton() {
  return (
    <div className="pt-stack-sm">
      <button
        className="w-full relative overflow-hidden bg-gradient-to-r from-primary-fixed to-primary-container text-on-primary-container font-label-md text-label-md py-3 px-4 rounded-lg flex items-center justify-center gap-2 shadow-sm shadow-primary/20 hover:shadow-md hover:-translate-y-0.5 transition-all duration-200 group"
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
          href="#"
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
        <UsernameField />
        <LoginButton />
      </form>
      <RegistrationLink />
    </div>
  );
}
