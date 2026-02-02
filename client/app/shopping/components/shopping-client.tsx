'use client';

import { useState } from 'react';

import { Product, ShoppingList } from '@/lib/domain';
import { ShoppingListMessage } from '@/lib/messages';

import ProductModalForm from './product-modal-form';
import ShoppingListSearchBar from './shopping-list-search-bar';
import ShoppingListTable from './shopping-list-table';

import styles from './shopping-client.module.css';

const StyleClasses = {
  ShoppingList: 'shopping-list',
};

export default function ShoppingClient({
  initialShoppingList,
  onRecord,
}: Readonly<{
  initialShoppingList: ShoppingListMessage;
  onRecord: (shoppingList: ShoppingListMessage) => Promise<void>;
}>) {
  const [products, setProducts] = useState<Product[]>(
    initialShoppingList.products.map(
      p => new Product(p.name, p.quantity, p.price, p.checked),
    ),
  );
  const [name, setName] = useState<string>(initialShoppingList.name ?? '');
  const [productToAdd, setProductToAdd] = useState<Product | undefined>();

  const shoppingList = new ShoppingList(products, name);

  const toggleChecked = (productName: string) => {
    setProducts(prev =>
      prev.map(p =>
        p.name === productName
          ? new Product(p.name, p.quantity, p.price, !p.checked)
          : p,
      ),
    );
  };

  const addProduct = (product: Product) => {
    setProducts(prev => [...prev, product]);
    setProductToAdd(undefined);
  };

  return (
    <main className={styles[StyleClasses.ShoppingList]}>
      <input
        className="text-3xl mb-4 text-center font-bold p-1 border rounded "
        placeholder="Lista de la compra"
        value={name}
        onChange={e => setName(e.target.value)}
      />
      <div className="flex justify-center items-center mb-4">
        <ShoppingListSearchBar onSearch={setProductToAdd} />
      </div>
      <ShoppingListTable shoppingList={shoppingList} onToggle={toggleChecked} />

      <button
        className="cursor-pointer p-2 border rounded border-gray-300 float-right mr-4"
        onClick={() =>
          onRecord({
            name: shoppingList.name,
            products: shoppingList.products.map(p => ({
              checked: p.checked,
              name: p.name,
              price: p.price,
              quantity: p.quantity,
            })),
          })
        }
      >
        Completar compra
      </button>

      {productToAdd && (
        <ProductModalForm
          product={productToAdd}
          shoppingList={shoppingList}
          confirm={addProduct}
          close={() => setProductToAdd(undefined)}
          editProduct={setProductToAdd}
        />
      )}
    </main>
  );
}
