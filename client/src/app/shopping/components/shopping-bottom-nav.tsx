type ShoppingBottomNavItemProps = {
  active?: boolean;
  href: string;
  icon: string;
  label: string;
};

function ShoppingBottomNavItem({
  active = false,
  href,
  icon,
  label,
}: Readonly<ShoppingBottomNavItemProps>) {
  const activeClass =
    'bg-primary-container text-on-primary-container rounded-2xl shadow-sm transition-transform active:scale-95';
  const inactiveClass =
    'text-on-surface-variant hover:text-primary transition-all';
  return (
    <a
      className={`flex flex-col items-center justify-center px-5 py-2 ${active ? activeClass : inactiveClass}`}
      href={href}
    >
      <span
        className="material-symbols-outlined"
        style={{ fontVariationSettings: active ? "'FILL' 1" : "'FILL' 0" }}
      >
        {icon}
      </span>
      <span className="text-[10px] font-bold font-label-sm mt-1">{label}</span>
    </a>
  );
}

const NAV_ITEMS: ShoppingBottomNavItemProps[] = [
  { href: '/markets', icon: 'storefront', label: 'Markets' },
  { active: true, href: '/shopping', icon: 'list_alt', label: 'Lists' },
  { href: '#', icon: 'group', label: 'Shared' },
  { href: '#', icon: 'person', label: 'Profile' },
];

export default function ShoppingBottomNav() {
  return (
    <nav className="md:hidden fixed bottom-0 left-0 w-full flex justify-around items-center px-4 pb-6 pt-2 bg-surface-container-lowest/90 backdrop-blur-xl border-t border-surface-variant/30 shadow-[0_-4px_12px_rgba(168,85,247,0.08)] z-50">
      {NAV_ITEMS.map(item => (
        <ShoppingBottomNavItem key={item.label} {...item} />
      ))}
    </nav>
  );
}
