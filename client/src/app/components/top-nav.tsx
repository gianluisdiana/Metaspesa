type NavActionButtonProps = { icon: string };

function NavActionButton({ icon }: Readonly<NavActionButtonProps>) {
  return (
    <button className="p-2 rounded-full hover:bg-orange-50 transition-colors scale-95 active:transition-transform text-slate-500 flex items-center justify-center">
      <span className="material-symbols-outlined">{icon}</span>
    </button>
  );
}

const ACTION_ICONS = ['account_circle', 'notifications'];

export function NavLogo() {
  return (
    <div className="text-2xl font-extrabold bg-gradient-to-r from-orange-400 to-yellow-500 bg-clip-text text-transparent">
      Metaspesa
    </div>
  );
}

export function NavActions() {
  return (
    <div className="flex items-center gap-2">
      {ACTION_ICONS.map(icon => (
        <NavActionButton key={icon} icon={icon} />
      ))}
    </div>
  );
}

export default function TopNav() {
  return (
    <nav className="fixed top-0 w-full z-50 border-b border-orange-100 bg-white/80 backdrop-blur-md text-orange-500 font-plus-jakarta text-sm font-medium shadow-sm shadow-purple-500/10">
      <div className="flex justify-between items-center px-6 h-16 w-full max-w-7xl mx-auto">
        <NavLogo />
        <NavActions />
      </div>
    </nav>
  );
}
