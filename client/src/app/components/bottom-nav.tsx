type BottomNavItemProps = {
  active?: boolean;
  href: string;
  icon: string;
  label: string;
};

function BottomNavItem({
  active = false,
  href,
  icon,
  label,
}: Readonly<BottomNavItemProps>) {
  const activeClass = 'bg-orange-100 text-orange-700 rounded-2xl';
  const inactiveClass = 'text-slate-400 hover:text-orange-400 transition-all';
  return (
    <a
      className={`flex flex-col items-center justify-center px-5 py-2 active:scale-90 transition-transform duration-150 ${active ? activeClass : inactiveClass}`}
      href={href}
    >
      <span
        className={`material-symbols-outlined mb-1${active ? ' icon-fill' : ''}`}
      >
        {icon}
      </span>
      {label}
    </a>
  );
}

const NAV_ITEMS: BottomNavItemProps[] = [
  { href: '/markets', icon: 'storefront', label: 'Markets' },
  { href: '/shopping', icon: 'list_alt', label: 'Lists' },
  { href: '/evolution', icon: 'monitoring', label: 'Evolution' },
  { href: '#', icon: 'person', label: 'Profile' },
];

export default function BottomNav({
  activeHref,
}: Readonly<{ activeHref: string }>) {
  return (
    <nav className="md:hidden fixed bottom-0 left-0 w-full flex justify-around items-center px-4 pb-6 pt-2 bg-white/90 backdrop-blur-xl text-orange-500 text-[10px] font-bold font-plus-jakarta z-50 border-t border-orange-50 shadow-[0_-4px_12px_rgba(168,85,247,0.08)]">
      {NAV_ITEMS.map(item => (
        <BottomNavItem
          key={item.label}
          {...item}
          active={item.href === activeHref}
        />
      ))}
    </nav>
  );
}
