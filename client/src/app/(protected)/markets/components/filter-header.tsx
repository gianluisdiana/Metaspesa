'use client';

import { useRouter, useSearchParams } from 'next/navigation';
import { useCallback } from 'react';

interface Props {
  marketNames: string[];
}

function SearchBar({
  value,
  onChange,
}: Readonly<{ value: string; onChange: (v: string) => void }>) {
  return (
    <div className="relative grow md:max-w-md">
      <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline">
        search
      </span>
      <input
        className="w-full bg-surface-container-highest border-none rounded-full py-2 pl-10 pr-4 text-body-md font-body-md text-on-surface placeholder:text-on-surface-variant focus:ring-2 focus:ring-primary focus:bg-surface-container-lowest transition-all shadow-inner"
        placeholder="Search products..."
        type="text"
        value={value}
        onChange={e => onChange(e.target.value)}
      />
    </div>
  );
}

function MarketSelect({
  value,
  options,
  onChange,
}: Readonly<{
  value: string;
  options: string[];
  onChange: (v: string) => void;
}>) {
  return (
    <select
      className="bg-surface-container border border-outline-variant rounded-full px-4 py-2 font-label-md text-label-md text-on-surface hover:bg-surface-container-high transition-colors cursor-pointer"
      value={value}
      onChange={e => onChange(e.target.value)}
    >
      <option value="">All markets</option>
      {options.map(name => (
        <option key={name} value={name}>
          {name}
        </option>
      ))}
    </select>
  );
}

function BrandFilter({
  value,
  onChange,
}: Readonly<{ value: string; onChange: (v: string) => void }>) {
  return (
    <div className="relative">
      <input
        className="bg-surface-container border border-outline-variant rounded-full px-4 py-2 font-label-md text-label-md text-on-surface placeholder:text-on-surface-variant hover:bg-surface-container-high focus:ring-2 focus:ring-primary transition-all"
        placeholder="Brand..."
        type="text"
        value={value}
        onChange={e => onChange(e.target.value)}
      />
    </div>
  );
}

export default function FilterHeader({ marketNames }: Readonly<Props>) {
  const router = useRouter();
  const searchParams = useSearchParams();

  const nameSegment = searchParams.get('name_segment') ?? '';
  const marketName = searchParams.get('market_name') ?? '';
  const brandName = searchParams.get('brand_name') ?? '';

  const updateParam = useCallback(
    (key: string, value: string) => {
      const params = new URLSearchParams(searchParams.toString());
      if (value) {
        params.set(key, value);
      } else {
        params.delete(key);
      }
      router.replace(`?${params.toString()}`);
    },
    [router, searchParams],
  );

  return (
    <div className="sticky top-16 z-30 bg-surface/90 backdrop-blur-md border-b border-surface-variant px-container-margin py-stack-md flex flex-col gap-stack-sm shadow-sm shadow-secondary/5">
      <div className="flex items-center justify-between">
        <h1 className="font-headline-lg text-headline-lg text-on-surface">
          {marketName || 'All Markets'}
        </h1>
      </div>
      <div className="flex flex-wrap gap-unit mt-unit items-center">
        <SearchBar
          value={nameSegment}
          onChange={v => updateParam('name_segment', v)}
        />
        <MarketSelect
          value={marketName}
          options={marketNames}
          onChange={v => updateParam('market_name', v)}
        />
        <BrandFilter
          value={brandName}
          onChange={v => updateParam('brand_name', v)}
        />
      </div>
    </div>
  );
}
