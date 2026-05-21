/* @vitest-environment jsdom */
import ProductCard from '@/app/(protected)/markets/components/product-card';
import { cleanup, fireEvent, render, screen } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';

import './next-image-mock';

const product = {
  category: 'Dairy',
  id: 'milk',
  imageAlt: 'Milk bottle',
  imageUrl: 'https://example.test/milk.png',
  name: 'Whole Milk',
  price: 'EUR 1.29',
  unit: '1 l',
};

function renderProductCard(onAdd = vi.fn()) {
  render(<ProductCard product={product} onAdd={onAdd} />);
  return { onAdd };
}

describe('product card component', () => {
  afterEach(cleanup);

  it('renders product image source', () => {
    renderProductCard();

    expect(screen.getByRole('img', { name: 'Milk bottle' })).toHaveAttribute(
      'src',
      'https://example.test/milk.png',
    );
  });

  it('renders product category', () => {
    renderProductCard();

    expect(screen.getByText('Dairy')).toBeInTheDocument();
  });

  it('renders product name', () => {
    renderProductCard();

    expect(screen.getByText('Whole Milk')).toBeInTheDocument();
  });

  it('renders product price', () => {
    renderProductCard();

    expect(screen.getByText('EUR 1.29')).toBeInTheDocument();
  });

  it('renders product unit', () => {
    renderProductCard();

    expect(screen.getByText('1 l')).toBeInTheDocument();
  });

  it('calls add handler from add button', () => {
    const { onAdd } = renderProductCard();

    fireEvent.click(screen.getByRole('button', { name: /add/i }));

    expect(onAdd).toHaveBeenCalledOnce();
  });
});
