type SideNavItemProps = {
  active?: boolean;
  href: string;
  icon: string;
  label: string;
};

export function SideNavItem({
  active = false,
  href,
  icon,
  label,
}: Readonly<SideNavItemProps>) {
  const activeClass = 'bg-white text-orange-700 rounded-lg shadow-sm font-bold';
  const inactiveClass = 'text-slate-600 hover:bg-orange-100/50 transition-all';
  return (
    <a
      className={`flex items-center gap-3 px-4 py-3 hover:translate-x-1 duration-200 ${active ? activeClass : inactiveClass}`}
      href={href}
    >
      <span
        className={`material-symbols-outlined${active ? ' icon-fill' : ''}`}
      >
        {icon}
      </span>
      {label}
    </a>
  );
}

const MAIN_LINKS: SideNavItemProps[] = [
  { active: true, href: '/markets', icon: 'storefront', label: 'Markets' },
  { href: '/lists', icon: 'receipt_long', label: 'Shopping Lists' },
];

const FOOTER_LINKS: SideNavItemProps[] = [
  { href: '#', icon: 'group', label: 'Shared with Me' },
  { href: '#', icon: 'settings', label: 'Settings' },
];

export function SideNavHeader() {
  return (
    <div className="mb-6 px-2 mt-4">
      <div className="flex items-center gap-3 mb-2">
        <div className="w-10 h-10 rounded-full bg-gradient-to-tr from-primary-container to-secondary-container flex items-center justify-center text-on-primary-container font-headline-md shadow-sm">
          M
        </div>
        <div>
          <h2 className="text-xl font-black text-orange-600">Metaspesa</h2>
          <p className="text-[11px] text-slate-500 font-medium">
            Your serene grocery assistant
          </p>
        </div>
      </div>
      <button className="w-full mt-4 py-2.5 px-4 bg-white border border-orange-200 text-orange-700 rounded-lg text-sm font-bold shadow-sm hover:bg-orange-50 hover:shadow transition-all flex items-center justify-center gap-2">
        <span className="material-symbols-outlined text-[18px]">add</span>
        Create New List
      </button>
    </div>
  );
}

export function MainNavLinks() {
  return (
    <div className="flex-1 flex flex-col gap-1">
      {MAIN_LINKS.map(link => (
        <SideNavItem key={link.href} {...link} />
      ))}
    </div>
  );
}

export function FooterNavLinks() {
  return (
    <div className="mt-auto border-t border-orange-100 pt-4 flex flex-col gap-1">
      {FOOTER_LINKS.map(link => (
        <SideNavItem key={link.href} {...link} />
      ))}
    </div>
  );
}

export default function SideNav() {
  return (
    <aside className="hidden md:flex flex-col h-screen p-4 gap-2 w-72 border-r border-orange-100 bg-orange-50/95 backdrop-blur-lg text-orange-600 font-plus-jakarta shadow-xl shadow-purple-900/5 fixed top-16 left-0 z-40 pb-20">
      <SideNavHeader />
      <MainNavLinks />
      <FooterNavLinks />
    </aside>
  );
}
