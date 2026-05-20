import { ProductBadge } from './product-card-badge';
import { type Product } from './product-card-model';

export function ProductImage({ product }: Readonly<{ product: Product }>) {
  return (
    <div className="aspect-4/3 rounded-lg bg-surface-container-high overflow-hidden relative">
      <img
        alt={product.imageAlt}
        className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
        src={product.imageUrl}
      />
      {product.badge && (
        <ProductBadge
          label={product.badge.label}
          colorClass={product.badge.colorClass}
        />
      )}
    </div>
  );
}
