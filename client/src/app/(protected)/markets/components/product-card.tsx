import { ProductActions } from './product-card-actions';
import { ProductImage } from './product-card-image';
import { type Product } from './product-card-model';
import { PriceDisplay } from './product-card-price';

export default function ProductCard({
  onAdd,
  product,
}: Readonly<{ onAdd: () => void; product: Product }>) {
  return (
    <div className="bg-surface-container-lowest rounded-xl p-3 flex flex-col gap-3 shadow-[0_4px_16px_rgba(97,88,136,0.08)] hover:shadow-[0_8px_24px_rgba(97,88,136,0.12)] transition-shadow duration-300 relative group overflow-hidden">
      <ProductImage product={product} />
      <div className="flex flex-col flex-1 px-1">
        <span className="text-xs font-medium text-on-surface-variant mb-1">
          {product.category}
        </span>
        <h3 className="font-headline-md text-[18px] leading-tight text-on-surface mb-1">
          {product.name}
        </h3>
        <div className="flex items-end justify-between mt-auto pt-2">
          <PriceDisplay product={product} />
          <ProductActions product={product} onAdd={onAdd} />
        </div>
      </div>
    </div>
  );
}
