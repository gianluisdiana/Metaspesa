import { FooterNavLinks, MainNavLinks } from '../../components/side-nav';

export function ShoppingSideNavHeader() {
  return (
    <>
      <div className="mb-stack-lg px-4 pt-4">
        <h1 className="font-headline-md text-headline-md text-primary">
          Metaspesa
        </h1>
        <p className="font-label-sm text-label-sm text-on-surface-variant mt-1">
          Your serene grocery assistant
        </p>
      </div>
      <button className="mb-stack-lg mx-4 bg-gradient-to-r from-primary to-primary-fixed-dim text-on-primary font-label-md text-label-md py-3 px-4 rounded-xl shadow-[0_4px_14px_rgba(135,82,0,0.2)] hover:shadow-[0_6px_20px_rgba(135,82,0,0.3)] transition-all flex justify-center items-center gap-2">
        <span
          className="material-symbols-outlined"
          style={{ fontVariationSettings: "'FILL' 0" }}
        >
          add
        </span>
        Create New List
      </button>
    </>
  );
}

export function UserProfile() {
  return (
    <div className="mt-4 px-4 flex items-center gap-3">
      <div className="w-10 h-10 rounded-full bg-secondary-container flex items-center justify-center text-on-secondary-container border border-outline-variant/30">
        <span
          className="material-symbols-outlined"
          style={{ fontVariationSettings: "'FILL' 1" }}
        >
          person
        </span>
      </div>
      <div>
        <div className="font-label-md text-label-md text-on-surface">
          User Profile
        </div>
        <div className="font-label-sm text-label-sm text-on-surface-variant">
          Free Plan
        </div>
      </div>
    </div>
  );
}

export default function ShoppingSideNav() {
  return (
    <nav className="hidden md:flex flex-col h-screen p-4 gap-2 w-72 bg-surface-container-low/95 border-r border-surface-variant shadow-[0_0_40px_rgba(208,197,253,0.1)] z-40 relative">
      <ShoppingSideNavHeader />
      <MainNavLinks activeHref="/shopping" />
      <FooterNavLinks />
      <UserProfile />
    </nav>
  );
}
