import { ToastProvider } from '@/app/(protected)/components/toast-provider';
import AddToListModal from '@/app/(protected)/markets/components/add-to-list-modal';
import FilterHeader from '@/app/(protected)/markets/components/filter-header';
import ProductGrid from '@/app/(protected)/markets/components/product-grid';
import ShoppingListContainer from '@/app/(protected)/shopping/components/shopping-list-container';
import { renderToStaticMarkup } from 'react-dom/server';
import { beforeEach, describe, expect, it, vi } from 'vitest';

const navigationMocks = vi.hoisted(() => ({
  pathname: '/markets',
  push: vi.fn(),
  replace: vi.fn(),
  searchParams: new URLSearchParams(),
}));

vi.mock('next/navigation', () => ({
  usePathname: () => navigationMocks.pathname,
  useRouter: () => ({
    push: navigationMocks.push,
    replace: navigationMocks.replace,
  }),
  useSearchParams: () => navigationMocks.searchParams,
}));

describe('component smoke tests', () => {
  beforeEach(() => {
    navigationMocks.pathname = '/markets';
    navigationMocks.push.mockReset();
    navigationMocks.replace.mockReset();
    navigationMocks.searchParams = new URLSearchParams();
  });

  it('renders add-to-list modal list choices', () => {
    const markup = renderToStaticMarkup(
      <AddToListModal
        isOpen
        productName="Milk"
        shoppingListSummaries={[{}, { name: 'Groceries' }]}
        onClose={() => undefined}
        onCreateList={() => undefined}
        onSelectList={() => undefined}
      />,
    );

    expect(markup).toContain('Add to List');
    expect(markup).toContain('Milk');
    expect(markup).toContain('Temporary List');
    expect(markup).toContain('Groceries');
  });

  it('does not render a closed add-to-list modal', () => {
    const markup = renderToStaticMarkup(
      <AddToListModal
        isOpen={false}
        shoppingListSummaries={[]}
        onClose={() => undefined}
        onCreateList={() => undefined}
        onSelectList={() => undefined}
      />,
    );

    expect(markup).toBe('');
  });

  it('renders an empty product grid state', () => {
    const markup = renderToStaticMarkup(
      <ToastProvider>
        <ProductGrid
          filter={{ page: 1, pageSize: 20 }}
          initialMarkets={[]}
          initialTotalProducts={0}
          shoppingListSummaries={[]}
        />
      </ToastProvider>,
    );

    expect(markup).toContain('No products found');
  });

  it('renders filter header options from search params', () => {
    navigationMocks.searchParams = new URLSearchParams({
      market_name: 'Mercadona',
      name_segment: 'milk',
    });

    const markup = renderToStaticMarkup(
      <FilterHeader marketNames={['Mercadona', 'Hiperdino']} />,
    );

    expect(markup).toContain('Mercadona');
    expect(markup).toContain('Hiperdino');
    expect(markup).toContain('value="milk"');
  });

  it('renders shopping list container summary state', () => {
    const markup = renderToStaticMarkup(
      <ToastProvider>
        <ShoppingListContainer
          initialShoppingList={{
            name: 'Groceries',
            products: [
              {
                checked: true,
                name: 'Milk',
                price: 1.25,
              },
            ],
          }}
          initialShoppingListSummaries={[{ name: 'Groceries' }]}
          initialSelectedListName="Groceries"
        />
      </ToastProvider>,
    );

    expect(markup).toContain('Groceries');
    expect(markup).toContain('1 item');
    expect(markup).toContain('$1.25');
  });
});
