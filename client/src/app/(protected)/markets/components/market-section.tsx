import { euros } from '@/lib/formatters/money-formatter';
import {
  MarketProductFormatMessage,
  MarketProductMessage,
} from '@/lib/market-contracts';

import ProductCard from './product-card';
import { type Product } from './product-card-model';

function toProduct(
  product: MarketProductMessage,
  format: MarketProductFormatMessage,
): Product {
  return {
    category: product.brand ?? '',
    id: `${product.id}:${format.id}`,
    imageAlt: product.name,
    imageUrl: format.imageUrl ?? '',
    name: product.name,
    price: format ? euros.format(format.currentPrice.amount) : '—',
    priceValue: format.currentPrice.amount,
    productFormatUid: format.id,
    unit: `${format.quantity.amount} ${format.quantity.unit}`,
  };
}

export function MarketSection({
  marketName,
  products,
  onAddProduct,
  showDivider,
}: Readonly<{
  marketName: string;
  products: MarketProductMessage[];
  onAddProduct: (product: Product) => void;
  showDivider: boolean;
}>) {
  return (
    <section className="flex flex-col gap-stack-sm">
      {showDivider && (
        <div className="flex items-center gap-4">
          <h2 className="font-headline-md text-headline-md whitespace-nowrap text-on-surface">
            {marketName}
          </h2>
          <div className="h-px flex-1 bg-outline-variant" />
        </div>
      )}
      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-gutter">
        {products.flatMap(item =>
          item.formats.map(format => {
            const product = toProduct(item, format);
            return (
              <ProductCard
                key={product.id}
                product={product}
                onAdd={() => onAddProduct(product)}
              />
            );
          }),
        )}
      </div>
    </section>
  );
}
