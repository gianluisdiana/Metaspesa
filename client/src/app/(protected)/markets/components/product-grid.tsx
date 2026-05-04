import { MarketMessage, MarketProductMessage } from '@/lib/market-messages';

import ProductCard, { type Product } from './product-card';

function toProduct(p: MarketProductMessage): Product {
  const [first] = p.formats;
  return {
    category: p.brandName,
    id: p.name,
    imageAlt: p.name,
    imageUrl: '',
    name: p.name,
    price: first ? `€${first.price.toFixed(2)}` : '—',
    unit: first?.quantity ?? '',
  };
}

function EmptyState() {
  return (
    <div className="flex flex-col items-center justify-center p-container-margin text-on-surface-variant">
      <span className="material-symbols-outlined text-[48px]">search_off</span>
      <p className="font-body-lg text-body-lg mt-2">No products found</p>
    </div>
  );
}

function MarketSection({ market }: Readonly<{ market: MarketMessage }>) {
  return (
    <section>
      <h2 className="font-title-lg text-title-lg text-on-surface mb-stack-sm">
        {market.name}
      </h2>
      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-gutter">
        {market.products.map(p => (
          <ProductCard key={p.name} product={toProduct(p)} />
        ))}
      </div>
    </section>
  );
}

export default function ProductGrid({
  markets,
}: Readonly<{ markets: MarketMessage[] }>) {
  if (markets.length === 0) {
    return <EmptyState />;
  }

  return (
    <div className="p-container-margin flex flex-col gap-section-gap">
      {markets.map(market => (
        <MarketSection key={market.name} market={market} />
      ))}
    </div>
  );
}
