'use client';

import { usePathname } from 'next/navigation';

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
  if (active) {
    return (
      <a
        className="flex items-center gap-3 bg-primary-container/20 text-on-primary-container rounded-lg px-4 py-3 shadow-sm border border-primary-container/30 relative overflow-hidden"
        href={href}
      >
        <div className="absolute left-0 top-0 bottom-0 w-1 bg-primary rounded-r-full" />
        <span
          className="material-symbols-outlined text-primary"
          style={{ fontVariationSettings: "'FILL' 1" }}
        >
          {icon}
        </span>
        <span className="font-label-md text-label-md font-bold">{label}</span>
      </a>
    );
  }
  return (
    <a
      className="flex items-center gap-3 text-on-surface-variant px-4 py-3 hover:bg-surface-container rounded-lg transition-colors group"
      href={href}
    >
      <span
        className="material-symbols-outlined group-hover:text-primary transition-colors"
        style={{ fontVariationSettings: "'FILL' 0" }}
      >
        {icon}
      </span>
      <span className="font-label-md text-label-md">{label}</span>
    </a>
  );
}

const MAIN_LINKS: SideNavItemProps[] = [
  { href: '/markets', icon: 'storefront', label: 'Markets' },
  { href: '/shopping', icon: 'receipt_long', label: 'Shopping Lists' },
  { href: '/evolution', icon: 'monitoring', label: 'Evolution' },
];

const FOOTER_LINKS: SideNavItemProps[] = [
  { href: '#', icon: 'settings', label: 'Settings' },
];

export function SideNavHeader() {
  return (
    <div className="mb-6 px-2 mt-4">
      <div className="flex items-center gap-3 mb-2">
        <div className="w-10 h-10 rounded-full bg-linear-to-tr from-primary-container to-secondary-container flex items-center justify-center text-on-primary-container font-headline-md shadow-sm">
          U
        </div>
        <div>
          <h2 className="text-xl font-black text-orange-600">Username</h2>
          <p className="text-[11px] text-slate-500 font-medium">
            username@example.com
          </p>
        </div>
      </div>
      <button className="w-full mt-4 py-2.5 px-4 bg-white border border-orange-200 text-orange-700 rounded-lg text-sm font-bold shadow-sm hover:bg-orange-50 hover:shadow transition-all flex items-center justify-center gap-2">
        <span className="material-symbols-outlined text-[18px]">add</span>
        {''}
        Create New List
      </button>
    </div>
  );
}

export function MainNavLinks() {
  const pathname = usePathname();
  return (
    <div className="flex-1 flex flex-col gap-unit">
      {MAIN_LINKS.map(link => (
        <SideNavItem
          key={link.href}
          {...link}
          active={link.href === pathname}
        />
      ))}
    </div>
  );
}

export function FooterNavLinks() {
  return (
    <div className="mt-auto border-t border-surface-variant pt-4 flex flex-col gap-unit">
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
