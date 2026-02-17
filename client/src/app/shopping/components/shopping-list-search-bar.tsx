'use client';

import { useEffect, useState } from 'react';

import FakeApiService from '@/infrastructure/fake-api-service';
import { Product } from '@/lib/domain';

export default function ShoppingListSearchBar({
  onSearch,
}: Readonly<{
  onSearch: (value: Product) => void;
}>) {
  const [inputValue, setInputValue] = useState('');
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [registeredProducts, setRegisteredProducts] = useState<Product[]>([]);

  useEffect(() => {
    new FakeApiService()
      .getRegisteredProducts()
      .then(products => setRegisteredProducts(products));
  }, []);

  const resetSearch = () => {
    setInputValue('');
    setShowSuggestions(false);
  };

  const filteredSuggestions = registeredProducts.filter(p =>
    p.name.toLowerCase().includes(inputValue.toLowerCase()),
  );

  return (
    <section>
      <article className="relative p-2 inline-block">
        <input
          type="text"
          placeholder="Añadir producto..."
          onKeyDown={e => {
            if (e.key === 'Enter') {
              onSearch(new Product(inputValue));
              resetSearch();
            }
          }}
          onChange={e => {
            setInputValue(e.currentTarget.value);
            setShowSuggestions(true);
          }}
          onBlur={() => setShowSuggestions(false)}
          onFocus={() => setShowSuggestions(true)}
          value={inputValue}
          className="border rounded border-gray-300 p-2"
        />

        {showSuggestions && filteredSuggestions.length > 0 && (
          <ShoppingListSuggestions
            key="suggestions-list"
            products={filteredSuggestions}
            onChange={(product: Product) => {
              onSearch(product);
              resetSearch();
            }}
          />
        )}
      </article>

      <button
        onClick={() => {
          onSearch(new Product(inputValue));
          resetSearch();
        }}
        className="border border-gray-300 rounded cursor-pointer p-2"
      >
        Añadir
      </button>
    </section>
  );
}

const ShoppingListSuggestions = ({
  products,
  onChange,
}: Readonly<{
  products: Product[];
  onChange: (value: Product) => void;
}>) => {
  const MaxAmountOfSuggestionsShown = 5;

  return (
    <div
      aria-label="Sugerencias de productos"
      className="bg-white border-gray-200 left-0 right-0 overflow-y-auto absolute top-full mt-1 z-10 shadow"
    >
      {products.slice(0, MaxAmountOfSuggestionsShown).map(product => (
        <ShoppingListSuggestionItem
          key={product.name}
          product={product}
          onItemSelected={onChange}
        />
      ))}
    </div>
  );
};

const ShoppingListSuggestionItem = ({
  product,
  onItemSelected,
}: Readonly<{
  product: Product;
  onItemSelected: (value: Product) => void;
}>) => {
  return (
    <option
      key={product.name}
      tabIndex={0}
      onMouseDown={() => onItemSelected(product)}
      style={{
        borderBottom: '1px solid rgba(0,0,0,0.04)',
        cursor: 'pointer',
        padding: '8px 12px',
      }}
      className="hover:bg-cyan-400/10 cursor-pointer px-3 py-2 border-b-gray-200"
    >
      {product.name}
      {product.price !== undefined && product.price !== null
        ? ` — ${product.price} €`
        : ''}
    </option>
  );
};
