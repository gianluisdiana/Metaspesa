import { type Product } from './product-card-model';

function QuantityControl() {
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

function CartButton() {
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

function AddButton({ onClick }: Readonly<{ onClick: () => void }>) {
  return (
    <button
      className="rounded-full btn-gradient flex items-center justify-center text-on-primary-fixed-variant hover:opacity-90 transition-opacity px-4 py-2 gap-2"
      title="Add to List"
      type="button"
      onClick={onClick}
    >
      <span className="material-symbols-outlined icon-fill text-[18px]">
        add
      </span>
      <span className="font-label-md text-sm">Add</span>
    </button>
  );
}

export function ProductActions({
  onAdd,
  product,
}: Readonly<{ onAdd: () => void; product: Product }>) {
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
      <AddButton onClick={onAdd} />
    </div>
  );
}
