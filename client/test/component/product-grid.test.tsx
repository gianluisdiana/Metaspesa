/* @vitest-environment jsdom */
import { ToastProvider } from '@/app/(protected)/components/toast-provider';
import ProductGrid from '@/app/(protected)/markets/components/product-grid';
import {
  cleanup,
  fireEvent,
  render,
  screen,
  within,
} from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

const navigationMocks = vi.hoisted(() => ({
  pathname: '/markets',
  push: vi.fn(),
}));

vi.mock('next/navigation', () => ({
  usePathname: () => navigationMocks.pathname,
  useRouter: () => ({
    push: navigationMocks.push,
  }),
}));

function renderAuthenticatedProductGrid() {
  render(
    <ToastProvider>
      <ProductGrid
        filter={{ page: 1, pageSize: 20 }}
        initialMarkets={[
          {
            name: 'Mercadona',
            products: [
              {
                brandName: 'Hacendado',
                formats: [
                  {
                    imageUrl: '',
                    price: 1.29,
                    quantity: '1 l',
                  },
                ],
                name: 'Whole Milk',
              },
            ],
          },
        ]}
        initialTotalProducts={1}
        isAuthenticated
        shoppingListSummaries={[{}, { name: 'Weekly' }]}
      />
    </ToastProvider>,
  );

  fireEvent.click(screen.getByRole('button', { name: /add/i }));
}

describe('product grid component', () => {
  beforeEach(() => {
    navigationMocks.pathname = '/markets';
    navigationMocks.push.mockReset();
  });
  afterEach(cleanup);

  it('opens add-to-list dialog when authenticated user adds product', () => {
    renderAuthenticatedProductGrid();

    expect(screen.getByRole('dialog')).toBeVisible();
  });

  it('shows selected product name in add-to-list dialog', () => {
    renderAuthenticatedProductGrid();

    expect(
      within(screen.getByRole('dialog')).getByText('Whole Milk'),
    ).toBeVisible();
  });
});
