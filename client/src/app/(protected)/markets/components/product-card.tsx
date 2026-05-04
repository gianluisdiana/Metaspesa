import Image from "next/image";

export type Product = {
  id: string;
  category: string;
  name: string;
  price: string;
  unit: string;
  imageUrl: string;
  imageAlt: string;
  badge?: { label: string; colorClass: string };
  originalPrice?: string;
  hasQuantityControl?: boolean;
};

export function ProductBadge({
  label,
  colorClass,
}: Readonly<{
  label: string;
  colorClass: string;
}>) {
  return (
    <div
      className={`absolute top-2 left-2 bg-white/90 backdrop-blur-sm px-2 py-0.5 rounded text-[10px] font-bold ${colorClass} shadow-sm tracking-wide uppercase`}
    >
      {label}
    </div>
  );
}

export function ProductImage({ product }: Readonly<{ product: Product }>) {
  return (
    <div className="aspect-4/3 rounded-lg bg-surface-container-high overflow-hidden relative">
      <Image
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

export function QuantityControl() {
  return (
    <div className="flex items-center bg-surface-container rounded-full shadow-inner border border-surface-container-high">
      <button className="w-8 h-8 flex items-center justify-center text-on-surface-variant hover:text-primary transition-colors">
        <span className="material-symbols-outlined text-[16px]">remove</span>
      </button>
      <span className="w-4 text-center font-label-md text-label-md text-on-surface">
        1
      </span>
      <button className="w-8 h-8 flex items-center justify-center text-on-surface-variant hover:text-primary transition-colors">
        <span className="material-symbols-outlined text-[16px]">add</span>
      </button>
    </div>
  );
}

export function CartButton() {
  return (
    <button
      className="w-10 h-10 rounded-full btn-gradient flex items-center justify-center text-on-primary-fixed-variant hover:opacity-90 transition-opacity"
      title="Add to List"
    >
      <span className="material-symbols-outlined icon-fill">
        add_shopping_cart
      </span>
    </button>
  );
}

export function AddButton() {
  return (
    <button
      className="rounded-full btn-gradient flex items-center justify-center text-on-primary-fixed-variant hover:opacity-90 transition-opacity px-4 py-2 gap-2"
      title="Add to List"
    >
      <span className="material-symbols-outlined icon-fill text-[18px]">
        add
      </span>
      <span className="font-label-md text-sm">Add</span>
    </button>
  );
}

export function ProductActions({ product }: Readonly<{ product: Product }>) {
  if (product.hasQuantityControl) {
    return (
      <div className="flex items-center gap-2">
        <QuantityControl />
        <CartButton />
      </div>
    );
  }
  return (
    <div className="flex items-center gap-2">
      <AddButton />
    </div>
  );
}

export default function ProductCard({
  product,
}: Readonly<{ product: Product }>) {
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
          <ProductActions product={product} />
        </div>
      </div>
    </div>
  );
}
