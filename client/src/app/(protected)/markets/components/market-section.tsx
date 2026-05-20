import { euros } from '@/lib/formatters/money-formatter';
import { MarketMessage, MarketProductMessage } from '@/lib/market-contracts';

import ProductCard from './product-card';
import { type Product } from './product-card-model';

const MISSING_PRICE_LABEL = '—';

function toProduct(p: MarketProductMessage): Product {
  const [first] = p.formats;
  return {
    category: p.brandName,
    id: p.name,
    imageAlt: p.name,
    imageUrl: first.imageUrl ?? '',
    name: p.name,
    price: first ? euros.format(first.price) : MISSING_PRICE_LABEL,
    unit: first?.quantity ?? '',
  };
}

function MarketDivider({ marketName }: Readonly<{ marketName: string }>) {
  return (
    <div className="flex items-center gap-4">
      <h2 className="font-headline-md text-headline-md whitespace-nowrap text-on-surface">
        {marketName}
      </h2>
      <div className="h-px flex-1 bg-outline-variant" />
    </div>
  );
}

export function MarketSection({
  market,
  onAddProduct,
  showDivider,
}: Readonly<{
  market: MarketMessage;
  onAddProduct: (product: Product) => void;
  showDivider: boolean;
}>) {
  return (
    <section className="flex flex-col gap-stack-sm">
      {showDivider && <MarketDivider marketName={market.name} />}
      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-gutter">
        {market.products.map(p => {
          const product = toProduct(p);
          return (
            <ProductCard
              key={p.name}
              product={product}
              onAdd={() => onAddProduct(product)}
            />
          );
        })}
      </div>
    </section>
  );
}
