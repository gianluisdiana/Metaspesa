import Image from 'next/image';

import { ProductBadge } from './product-card-badge';
import { type Product } from './product-card-model';

export function ProductImage({ product }: Readonly<{ product: Product }>) {
  return (
    <div className="aspect-4/3 rounded-lg bg-surface-container-high overflow-hidden relative">
      {product.imageUrl ? (
        <Image
          fill
          unoptimized
          alt={product.imageAlt}
          className="object-cover transition-transform duration-500 group-hover:scale-105"
          sizes="(min-width: 1280px) 20vw, (min-width: 1024px) 25vw, (min-width: 640px) 33vw, 50vw"
          src={product.imageUrl}
        />
      ) : (
        <div className="flex h-full w-full items-center justify-center text-on-surface-variant">
          <span className="material-symbols-outlined text-4xl">image</span>
        </div>
      )}
      {product.badge && (
        <ProductBadge
          label={product.badge.label}
          colorClass={product.badge.colorClass}
        />
      )}
    </div>
  );
}
