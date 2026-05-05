'use client';

import { usePathname, useRouter, useSearchParams } from 'next/navigation';
import { useCallback, useEffect, useState } from 'react';

const FILTER_DEBOUNCE_MS = 350;

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
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();

  const nameSegment = searchParams.get('name_segment') ?? '';
  const marketName = searchParams.get('market_name') ?? '';
  const brandName = searchParams.get('brand_name') ?? '';
  const [pendingNameSegment, setPendingNameSegment] = useState(nameSegment);
  const [pendingBrandName, setPendingBrandName] = useState(brandName);

  const replaceParams = useCallback(
    (values: Readonly<Record<string, string>>) => {
      const params = new URLSearchParams(searchParams.toString());
      Object.entries(values).forEach(([key, value]) => {
        if (value) {
          params.set(key, value);
        } else {
          params.delete(key);
        }
      });
      const queryString = params.toString();
      const nextUrl = queryString ? `${pathname}?${queryString}` : pathname;
      router.replace(nextUrl);
    },
    [pathname, router, searchParams],
  );

  useEffect(() => {
    setPendingNameSegment(nameSegment);
  }, [nameSegment]);

  useEffect(() => {
    setPendingBrandName(brandName);
  }, [brandName]);

  useEffect(() => {
    const timeoutId = globalThis.setTimeout(() => {
      if (
        pendingBrandName !== brandName ||
        pendingNameSegment !== nameSegment
      ) {
        replaceParams({
          brand_name: pendingBrandName,
          name_segment: pendingNameSegment,
        });
      }
    }, FILTER_DEBOUNCE_MS);

    return () => globalThis.clearTimeout(timeoutId);
  }, [
    brandName,
    nameSegment,
    pendingBrandName,
    pendingNameSegment,
    replaceParams,
  ]);

  return (
    <div className="sticky top-16 z-30 bg-surface/90 backdrop-blur-md border-b border-surface-variant px-container-margin py-stack-md flex flex-col gap-stack-sm shadow-sm shadow-secondary/5">
      <div className="flex items-center justify-between">
        <h1 className="font-headline-lg text-headline-lg text-on-surface">
          {marketName || 'All Markets'}
        </h1>
      </div>
      <div className="flex flex-wrap gap-unit mt-unit items-center">
        <SearchBar
          value={pendingNameSegment}
          onChange={setPendingNameSegment}
        />
        <MarketSelect
          value={marketName}
          options={marketNames}
          onChange={v => replaceParams({ market_name: v })}
        />
        <BrandFilter value={pendingBrandName} onChange={setPendingBrandName} />
      </div>
    </div>
  );
}
