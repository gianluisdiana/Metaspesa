'use client';

import Link from 'next/link';
import { useState } from 'react';

type NavActionButtonProps = { icon: string };

function NavActionButton({ icon }: Readonly<NavActionButtonProps>) {
  return (
    <button className="p-2 rounded-full hover:bg-orange-50 transition-colors scale-95 active:transition-transform text-slate-500 flex items-center justify-center">
      <span className="material-symbols-outlined">{icon}</span>
    </button>
  );
}

const LANGUAGES = ['EN', 'ES', 'IT'] as const;

export function NavLogo() {
  return (
    <div className="text-2xl font-extrabold bg-linear-to-r from-orange-400 to-yellow-500 bg-clip-text text-transparent">
      <Link href="/">Metaspesa</Link>
    </div>
  );
}

function LanguageSelector() {
  const [language, setLanguage] = useState<(typeof LANGUAGES)[number]>('EN');

  return (
    <label className="flex items-center gap-1 rounded-full p-2 text-slate-500 transition-colors hover:bg-orange-50">
      <span className="material-symbols-outlined text-[20px]">language</span>
      <select
        aria-label="Select language"
        className="bg-transparent text-xs font-bold outline-none"
        value={language}
        onChange={event =>
          setLanguage(event.target.value as (typeof LANGUAGES)[number])
        }
      >
        {LANGUAGES.map(option => (
          <option key={option} value={option}>
            {option}
          </option>
        ))}
      </select>
    </label>
  );
}

export function NavActions() {
  return (
    <div className="flex items-center gap-2">
      <LanguageSelector />
      <NavActionButton icon="account_circle" />
      <NavActionButton icon="notifications" />
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
