'use client';

import React, { useRef, useState } from 'react';
import { Product, ShoppingList } from '@/lib/domain';

export default function ProductModalForm({
  product,
  shoppingList,
  editProduct,
  confirm,
  close,
}: Readonly<{
  close: () => void;
  product: Product;
  shoppingList: ShoppingList;
  confirm: (product: Product) => void;
  editProduct: (product: Product) => void;
}>) {
  const [errors, setErrors] = useState<{
    name?: string;
    price?: string;
    quantity?: string;
  }>({});

  const nameRef = useRef<HTMLInputElement | null>(null);
  const priceRef = useRef<HTMLInputElement | null>(null);
  const quantityRef = useRef<HTMLInputElement | null>(null);

  const handleSubmit = (e?: React.FormEvent) => {
    e?.preventDefault();
    const next: typeof errors = {};

    if (!product.name.trim()) {
      next.name = 'El nombre es obligatorio';
    } else if (shoppingList.contains(product)) {
      next.name = 'El producto ya existe en la lista';
    }
    if (!product.hasValidPrice()) {
      next.price = 'El precio no puede ser negativo';
    }
    if (!product.hasValidQuantity()) {
      next.quantity = 'La cantidad debe tener 50 caracteres como máximo';
    }

    setErrors(next);

    if (Object.keys(next).length === 0) {
      confirm(product);
      return;
    }

    if (next.name) nameRef.current?.focus();
    else if (next.price) priceRef.current?.focus();
    else if (next.quantity) quantityRef.current?.focus();
  };

  return (
    <dialog
      open
      className="w-96 p-4 rounded bg-white left-1/2 top-1/2 transform -translate-x-1/2 -translate-y-1/2"
    >
      <form method="dialog">
        <section className="flex flex-col items-center">
          <input
            ref={nameRef}
            type="text"
            value={product.name}
            onChange={e =>
              editProduct(
                new Product(e.target.value, product.quantity, product.price),
              )
            }
            placeholder="Nombre del producto"
            required
            aria-invalid={!!errors.name}
            aria-describedby={errors.name ? 'name-error' : undefined}
            className={`font-bold text-2xl mb-2 text-center border rounded ${
              errors.name ? 'border-red-600' : 'border-gray-300'
            }`}
          />
          {errors.name && (
            <div
              id="name-error"
              role="alert"
              style={{ color: '#dc2626', marginBottom: 8 }}
            >
              {errors.name}
            </div>
          )}
        </section>

        <section>
          <ProductInputField<string>
            label="Cantidad"
            value={product.quantity ?? ''}
            setValue={value =>
              editProduct(new Product(product.name, value, product.price))
            }
            inputProps={{
              'aria-describedby': errors.quantity
                ? 'quantity-error'
                : undefined,
              'aria-invalid': !!errors.quantity,
              ref: quantityRef,
            }}
          />
          <ProductInputField<number>
            label="Precio"
            value={product.price ?? 0}
            setValue={value =>
              editProduct(new Product(product.name, product.quantity, value))
            }
            inputProps={{
              'aria-describedby': errors.price ? 'price-error' : undefined,
              'aria-invalid': !!errors.price,
              ref: priceRef,
            }}
          />
          {(errors.price ?? errors.quantity) && (
            <div
              id="error"
              role="alert"
              style={{ color: '#dc2626', margin: '4px 20px' }}
            >
              {errors.price && <div>{errors.price}</div>}
              {errors.quantity && <div>{errors.quantity}</div>}
            </div>
          )}
        </section>
        <ProductModalMenu close={close} confirm={handleSubmit} />
      </form>
    </dialog>
  );
}

const ProductInputField = <T extends string | number>({
  label,
  value,
  setValue,
  inputProps,
}: Readonly<{
  label: string;
  value: T;
  setValue: (value: T) => void;
  inputProps?: React.InputHTMLAttributes<HTMLInputElement> & {
    ref?: React.RefObject<HTMLInputElement | null>;
  };
}>) => {
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
        {...inputProps}
      />
    </article>
  );
};

const ProductModalMenu = ({
  close,
  confirm,
}: Readonly<{
  close: () => void;
  confirm: () => void;
}>) => {
  return (
    <menu className="flex justify-end gap-4 mr-5">
      <button type="button" className="cursor-pointer" onClick={close}>
        Cancel
      </button>
      <button type="submit" className="cursor-pointer" onClick={confirm}>
        Confirm
      </button>
    </menu>
  );
};
