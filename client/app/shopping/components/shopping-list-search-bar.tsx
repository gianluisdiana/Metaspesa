'use client';

import { useState } from 'react';
import { Product } from '../Product';

export const ShoppingListSearchBar = ({
  onSearch,
}: {
  onSearch: (value: Product) => void;
}) => {
  const [inputValue, setInputValue] = useState('');
  const [showSuggestions, setShowSuggestions] = useState(false);

  const resetSearch = () => {
    setInputValue('');
    setShowSuggestions(false);
  };

  const defaultProduct: Product = {
    checked: false,
    name: '',
    price: 0,
    quantity: '',
  };

  const filteredSuggestions = registeredProducts.filter(p =>
    p.name.toLowerCase().includes(inputValue.toLowerCase()),
  );

  return (
    <div>
      <div
        style={{
          display: 'inline-block',
          paddingRight: 8,
          position: 'relative',
        }}
      >
        <input
          type="text"
          placeholder="Añadir producto..."
          onKeyDown={e => {
            if (e.key === 'Enter') {
              onSearch({ ...defaultProduct, name: inputValue });
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
        />

        {showSuggestions && filteredSuggestions.length > 0 && (
          <ShoppingListSuggestions
            key="suggestions-list"
            products={filteredSuggestions}
            onSuggestionSelected={onSearch}
            resetSearch={resetSearch}
          />
        )}
      </div>

      <button
        onClick={() => {
          onSearch({ ...defaultProduct, name: inputValue });
          resetSearch();
        }}
        style={{
          border: '1px solid #ccc',
          borderRadius: '4px',
          cursor: 'pointer',
        }}
      >
        Añadir
      </button>
    </div>
  );
};

const ShoppingListSuggestions = ({
  products,
  onSuggestionSelected,
  resetSearch,
}: {
  products: Product[];
  onSuggestionSelected: (value: Product) => void;
  resetSearch: () => void;
}) => {
  const MaxAmountOfSuggestionsShown = 5;

  const onChange = (product: Product) => {
    onSuggestionSelected(product);
    resetSearch();
  };

  return (
    <div
      aria-label="Sugerencias de productos"
      style={{
        background: 'white',
        border: '1px solid #ccc',
        boxShadow: '0 2px 6px rgba(0,0,0,0.12)',
        left: 0,
        marginTop: 6,
        maxHeight: 200,
        overflowY: 'auto',
        position: 'absolute',
        right: 0,
        top: '100%',
        zIndex: 10,
      }}
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
}: {
  product: Product;
  onItemSelected: (value: Product) => void;
}) => {
  return (
    <option
      key={product.name}
      tabIndex={0}
      onMouseDown={() => onItemSelected(product)}
      onKeyDown={e => {
        if (e.key === 'Enter' || e.key === ' ') {
          onItemSelected(product);
        }
      }}
      style={{
        borderBottom: '1px solid rgba(0,0,0,0.04)',
        cursor: 'pointer',
        padding: '8px 12px',
      }}
      className='hover:bg-cyan-400/10 cursor-pointer px-3 py-2 border-b-gray-200'
    >
      <strong>{product.name}</strong>
      {product.price !== undefined && product.price !== null
        ? ` — ${product.price} €`
        : ''}
    </option>
  );
};

const registeredProducts: Product[] = [
  { checked: false, name: 'Manzanas', price: 2, quantity: '1 paquete' },
  { checked: false, name: 'Pan', price: 1.5, quantity: '2 barras' },
  { checked: false, name: 'Leche', quantity: '1 litro' },
];
