type EvolutionNavItemProps = {
  active?: boolean;
  href: string;
  icon: string;
  label: string;
};

function EvolutionNavItem({
  active = false,
  href,
  icon,
  label,
}: Readonly<EvolutionNavItemProps>) {
  const activeClass =
    'bg-orange-100 text-orange-700 rounded-2xl px-5 py-2 active:scale-90 transition-transform duration-150';
  const inactiveClass =
    'text-slate-400 px-5 py-2 hover:text-orange-400 transition-all active:scale-90 duration-150';
  return (
    <a
      className={`flex flex-col items-center justify-center ${active ? activeClass : inactiveClass}`}
      href={href}
    >
      <span
        className="material-symbols-outlined"
        style={active ? { fontVariationSettings: "'FILL' 1" } : undefined}
      >
        {icon}
      </span>
      <span className="text-[10px] font-bold font-plus-jakarta mt-1">
        {label}
      </span>
    </a>
  );
}

const NAV_ITEMS: EvolutionNavItemProps[] = [
  { href: '/markets', icon: 'storefront', label: 'Markets' },
  { href: '/shopping', icon: 'list_alt', label: 'Lists' },
  { active: true, href: '/evolution', icon: 'monitoring', label: 'Evolution' },
  { href: '#', icon: 'person', label: 'Profile' },
];

export default function EvolutionBottomNav() {
  return (
    <nav className="md:hidden fixed bottom-0 left-0 w-full flex justify-around items-center px-4 pb-6 pt-2 bg-white/90 backdrop-blur-xl z-50 border-t border-orange-50 shadow-[0_-4px_12px_rgba(168,85,247,0.08)]">
      {NAV_ITEMS.map(item => (
        <EvolutionNavItem key={item.label} {...item} />
      ))}
    </nav>
  );
}
