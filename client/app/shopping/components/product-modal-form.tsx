'use client';

import { Product } from '../domain';

export default function ProductModalForm({
  product,
  editProduct,
  confirm,
  close,
}: {
  close: () => void;
  product: Product;
  confirm: (product: Product) => void;
  editProduct: (product: Product) => void;
}) {
  return (
    <dialog
      open
      className="w-96 p-4 rounded bg-white left-1/2 top-1/2 transform -translate-x-1/2 -translate-y-1/2"
    >
      <form method="dialog">
        <section className="flex flex-col items-center">
          <input
            type="text"
            value={product.name}
            onChange={e => editProduct({ ...product, name: e.target.value })}
            placeholder="Nombre del producto"
            required
            className="font-bold text-2xl mb-4 text-center border border-gray-300 rounded"
          />
        </section>
        <section>
          <ProductInputField<string>
            label="Cantidad"
            value={product.quantity ?? ''}
            setValue={value => editProduct({ ...product, quantity: value })}
          />
          <ProductInputField<number>
            label="Precio"
            value={product.price ?? 0}
            setValue={value => editProduct({ ...product, price: value })}
          />
        </section>
        <ProductModalMenu close={close} confirm={() => confirm(product)} />
      </form>
    </dialog>
  );
}

const ProductInputField = <T extends string | number>({
  label,
  value,
  setValue,
}: {
  label: string;
  value: T;
  setValue: (value: T) => void;
}) => {
  const inputType = typeof value === 'number' ? 'number' : 'text';
  const inputConverter = typeof value === 'number' ? Number : String;

  return (
    <article className="flex justify-between mb-3 mx-5">
      <label htmlFor={label.toLowerCase()} className="font-semibold py-1">
        {label}:
      </label>
      <input
        id={label.toLowerCase()}
        className="border border-gray-300 rounded px-2 py-1"
        type={inputType}
        value={value}
        onChange={e => setValue(inputConverter(e.target.value) as T)}
      />
    </article>
  );
};

const ProductModalMenu = ({
  close,
  confirm,
}: {
  close: () => void;
  confirm: () => void;
}) => {
  return (
    <menu className="flex justify-end gap-4 mr-5">
      <button type="reset" className="cursor-pointer" onClick={close}>
        Cancel
      </button>
      <button type="submit" className="cursor-pointer" onClick={confirm}>
        Confirm
      </button>
    </menu>
  );
};
