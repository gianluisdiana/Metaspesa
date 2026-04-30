export function MobileLogoTitle() {
  return (
    <div className="md:hidden text-2xl font-extrabold bg-gradient-to-r from-primary to-primary-fixed-dim bg-clip-text text-transparent font-headline-md tracking-tight">
      Metaspesa
    </div>
  );
}

export function DesktopTitle() {
  return (
    <div className="hidden md:block font-headline-md text-headline-md text-on-surface">
      Lists Dashboard
    </div>
  );
}

export function HeaderActions() {
  return (
    <div className="flex items-center gap-4 text-on-surface-variant">
      <button className="p-2 rounded-full hover:bg-surface-container transition-colors relative">
        <span
          className="material-symbols-outlined"
          style={{ fontVariationSettings: "'FILL' 0" }}
        >
          notifications
        </span>
        <span className="absolute top-2 right-2 w-2 h-2 bg-error rounded-full border border-surface" />
      </button>
      <button className="p-2 rounded-full hover:bg-surface-container transition-colors md:hidden">
        <span
          className="material-symbols-outlined"
          style={{ fontVariationSettings: "'FILL' 0" }}
        >
          account_circle
        </span>
      </button>
    </div>
  );
}

export default function ShoppingHeader() {
  return (
    <header className="fixed top-0 w-full md:w-[calc(100%-18rem)] z-50 border-b border-surface-variant/50 bg-surface/80 backdrop-blur-md shadow-[0_2px_10px_rgba(208,197,253,0.05)] transition-all">
      <div className="flex justify-between items-center px-6 h-16 w-full max-w-7xl mx-auto">
        <div className="flex items-center gap-4">
          <MobileLogoTitle />
          <DesktopTitle />
        </div>
        <HeaderActions />
      </div>
    </header>
  );
}
