'use client';

import { useState } from 'react';
import styles from './page.module.css';
import { Product } from './Product';
import { ShoppingListTable } from './components/shopping-list-table';
import { ShoppingListSearchBar } from './components/shopping-list-search-bar';

const StyleClasses = {
  SearchBar: 'shopping-list-search-bar',
  ShoppingList: 'shopping-list',
};

export default function Home() {
  const [products, setProducts] = useState<Product[]>(sampleProducts);
  const [productToAdd, setProductToAdd] = useState<Product>();

  const toggleChecked = (name: string) => {
    setProducts(prev =>
      prev.map(p => (p.name === name ? { ...p, checked: !p.checked } : p)),
    );
  };

  return (
    <div>
      <main className={styles[StyleClasses.ShoppingList]}>
        <h1 style={{ textAlign: 'center' }}>Lista de la compra</h1>
        <div className={styles[StyleClasses.SearchBar]}>
          <ShoppingListSearchBar onSearch={setProductToAdd} />
        </div>
        <ShoppingListTable products={products} onToggle={toggleChecked} />
      </main>
      {productToAdd && (
        <div>
          <h2>Producto buscado: {productToAdd.name}</h2>
        </div>
      )}
    </div>
  );
}

const sampleProducts: Product[] = [
  { checked: false, name: 'Manzanas', price: 2, quantity: '1 paquete' },
  { checked: true, name: 'Pan', price: 1.5, quantity: '2 barras' },
  { checked: false, name: 'Leche', quantity: '1 litro' },
];
