'use client';

import { useEffect, useMemo, useState } from 'react';

import FakeApiService from '@/infrastructure/fake-api-service';
import ApiService from '@/lib/api-service';
import { Product, ShoppingList } from '@/lib/domain';

import ProductModalForm from './components/product-modal-form';
import ShoppingListSearchBar from './components/shopping-list-search-bar';
import ShoppingListTable from './components/shopping-list-table';

import styles from './page.module.css';

const StyleClasses = {
  ShoppingList: 'shopping-list',
};

export default function Home() {
  const [products, setProducts] = useState<Product[]>([]);
  const [name, setName] = useState<string>('');
  const [productToAdd, setProductToAdd] = useState<Product | undefined>();

  const apiService: ApiService = useMemo(() => new FakeApiService(), []);

  useEffect(() => {
    apiService.getCurrentShoppingList().then(shoppingList => {
      setProducts(shoppingList.products);
      setName(shoppingList.name ?? '');
    });
  }, [setProducts, apiService]);

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
        onClick={() => apiService.recordShoppingList(shoppingList)}
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
