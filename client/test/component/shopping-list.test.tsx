/* @vitest-environment jsdom */
import { ToastProvider } from '@/app/(protected)/components/toast-provider';
import ShoppingListContainer from '@/app/(protected)/shopping/components/shopping-list-container';
import { cleanup, render, screen } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';

const navigationMocks = vi.hoisted(() => ({
  replace: vi.fn(),
}));

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    replace: navigationMocks.replace,
  }),
}));

function renderShoppingList() {
  render(
    <ToastProvider>
      <ShoppingListContainer
        initialSelectedListName="Groceries"
        initialShoppingList={{
          name: 'Groceries',
          products: [
            {
              checked: false,
              name: 'Milk',
              price: 1.25,
              quantity: '1 l',
            },
            { checked: true, name: 'Bread', price: 2.35 },
          ],
        }}
        initialShoppingListSummaries={[{ name: 'Groceries' }]}
      />
    </ToastProvider>,
  );
}

describe('shopping list component', () => {
  afterEach(cleanup);

  it('renders selected shopping list name', () => {
    renderShoppingList();

    expect(screen.getByRole('heading', { name: 'Groceries' })).toBeVisible();
  });

  it('renders item count label', () => {
    renderShoppingList();

    expect(screen.getByText('2 items')).toBeVisible();
  });

  it('renders unchecked item name', () => {
    renderShoppingList();

    expect(screen.getByText('Milk')).toBeVisible();
  });

  it('renders checked item name', () => {
    renderShoppingList();

    expect(screen.getAllByText('Bread')[0]).toBeVisible();
  });

  it('renders estimated total', () => {
    renderShoppingList();

    expect(screen.getByText('$3.60')).toBeVisible();
  });

  it('renders checked total', () => {
    renderShoppingList();

    expect(screen.getAllByText('$2.35')[0]).toBeVisible();
  });
});
