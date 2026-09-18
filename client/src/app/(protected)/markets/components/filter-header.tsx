'use client';

import { usePathname, useRouter, useSearchParams } from 'next/navigation';
import { useEffect, useState } from 'react';

import RestMarketApiService from '@/infrastructure/rest-market-api-service';
import { MarketSummaryMessage } from '@/lib/market-contracts';

import { RetryButton } from './product-grid-states';

const FILTER_DEBOUNCE_MS = 350;

interface Props {
  markets?: MarketSummaryMessage[];
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
  options: MarketSummaryMessage[];
  onChange: (v: string) => void;
}>) {
  return (
    <select
      className="bg-surface-container border border-outline-variant rounded-full px-4 py-2 font-label-md text-label-md text-on-surface hover:bg-surface-container-high transition-colors cursor-pointer"
      value={value}
      onChange={e => onChange(e.target.value)}
    >
      <option value="">All markets</option>
      {options.map(market => (
        <option key={market.id} value={market.id}>
          {market.name}
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

function FilterControls({
  brandName,
  marketId,
  markets,
  query,
  sort,
  replaceParams,
}: Readonly<{
  brandName: string;
  marketId: string;
  markets: MarketSummaryMessage[];
  query: string;
  sort: string;
  replaceParams: (values: Readonly<Record<string, string>>) => void;
}>) {
  const [pendingQuery, setPendingQuery] = useState(query);
  const [pendingBrandName, setPendingBrandName] = useState(brandName);

  useEffect(() => {
    const timeoutId = globalThis.setTimeout(() => {
      if (pendingBrandName !== brandName || pendingQuery !== query) {
        replaceParams({
          brand: pendingBrandName,
          query: pendingQuery,
        });
      }
    }, FILTER_DEBOUNCE_MS);

    return () => globalThis.clearTimeout(timeoutId);
  }, [brandName, query, pendingBrandName, pendingQuery, replaceParams]);

  return (
    <div className="flex flex-wrap gap-unit mt-unit items-center">
      <SearchBar value={pendingQuery} onChange={setPendingQuery} />
      <MarketSelect
        value={marketId}
        options={markets}
        onChange={value => replaceParams({ marketId: value })}
      />
      <BrandFilter value={pendingBrandName} onChange={setPendingBrandName} />
      <select
        aria-label="Sort products"
        className="bg-surface-container border border-outline-variant rounded-full px-4 py-2 font-label-md text-label-md text-on-surface"
        value={sort}
        onChange={event => replaceParams({ sort: event.target.value })}
      >
        <option value="name">Name</option>
        <option value="priceAsc">Price: low to high</option>
        <option value="priceDesc">Price: high to low</option>
      </select>
    </div>
  );
}

export default function FilterHeader({ markets }: Readonly<Props>) {
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();
  const [fetchedMarkets, setFetchedMarkets] = useState<MarketSummaryMessage[]>(
    [],
  );
  const [marketsFailed, setMarketsFailed] = useState(false);
  const [retry, setRetry] = useState(0);
  const availableMarkets = markets ?? fetchedMarkets;

  useEffect(() => {
    if (markets) return;
    let cancelled = false;
    new RestMarketApiService().getMarkets().then(
      items => {
        if (!cancelled) setFetchedMarkets(items);
      },
      () => {
        if (!cancelled) setMarketsFailed(true);
      },
    );
    return () => {
      cancelled = true;
    };
  }, [markets, retry]);

  const query = searchParams.get('query') ?? '';
  const marketId = searchParams.get('marketId') ?? '';
  const brandName = searchParams.get('brand') ?? '';
  const sort = searchParams.get('sort') ?? 'name';
  const marketName = availableMarkets.find(
    market => String(market.id) === marketId,
  )?.name;

  function replaceParams(values: Readonly<Record<string, string>>) {
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
  }

  return (
    <div className="sticky top-16 z-30 bg-surface/90 backdrop-blur-md border-b border-surface-variant px-container-margin py-stack-md flex flex-col gap-stack-sm shadow-sm shadow-secondary/5">
      <div className="flex items-center justify-between">
        <h1 className="font-headline-lg text-headline-lg text-on-surface">
          {marketName ?? 'All markets'}
        </h1>
      </div>
      <FilterControls
        key={JSON.stringify([query, brandName])}
        brandName={brandName}
        marketId={marketId}
        markets={availableMarkets}
        query={query}
        replaceParams={replaceParams}
        sort={sort}
      />
      {marketsFailed && (
        <RetryButton
          onRetry={() => {
            setMarketsFailed(false);
            setRetry(value => value + 1);
          }}
        />
      )}
    </div>
  );
}
