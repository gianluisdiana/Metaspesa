'use client';

import { useEffect, useState } from 'react';
import styles from './page.module.css';
import { Product, ShoppingList } from '@/lib/domain';
import { ShoppingListTable } from './components/shopping-list-table';
import ShoppingListSearchBar from './components/shopping-list-search-bar';
import ProductModalForm from './components/product-modal-form';
import FakeApiService from '@/infrastructure/fake-api-service';

const StyleClasses = {
  SearchBar: 'shopping-list-search-bar',
  ShoppingList: 'shopping-list',
};

export default function Home() {
  const [products, setProducts] = useState<Product[]>([]);
  const [productToAdd, setProductToAdd] = useState<Product | undefined>();

  useEffect(() => {
    new FakeApiService()
      .getCurrentShoppingList()
      .then(shoppingList => setProducts(shoppingList.products));
  }, [setProducts]);

  const shoppingList = new ShoppingList(products);

  const toggleChecked = (name: string) => {
    setProducts(prev =>
      prev.map(p => (p.name === name ? { ...p, checked: !p.checked } : p)),
    );
  };

  const addProduct = (product: Product) => {
    setProducts(prev => [...prev, product]);
    setProductToAdd(undefined);
  };

  return (
    <div>
      <main className={styles[StyleClasses.ShoppingList]}>
        <h1 className="text-3xl mb-4">Lista de la compra</h1>
        <div className="flex justify-center items-center mb-4">
          <ShoppingListSearchBar onSearch={setProductToAdd} />
        </div>
        <ShoppingListTable
          shoppingList={shoppingList}
          onToggle={toggleChecked}
        />
      </main>

      {productToAdd && !shoppingList.contains(productToAdd) && (
        <div className="fixed top-0 left-0 w-full h-full bg-black/30 flex justify-center items-center">
          <ProductModalForm
            product={productToAdd}
            confirm={addProduct}
            close={() => setProductToAdd(undefined)}
            editProduct={setProductToAdd}
          />
        </div>
      )}
    </div>
  );
}
