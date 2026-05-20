import { type Product } from './product-card-model';

export function PriceDisplay({ product }: Readonly<{ product: Product }>) {
  return (
    <div>
      <div className="flex items-center gap-2">
        <p className="font-headline-lg text-xl text-primary">{product.price}</p>
        {product.originalPrice && (
          <p className="text-sm text-outline-variant line-through">
            {product.originalPrice}
          </p>
        )}
      </div>
      <p className="text-xs text-on-surface-variant">{product.unit}</p>
    </div>
  );
}
