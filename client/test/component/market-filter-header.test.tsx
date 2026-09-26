/* @vitest-environment jsdom */
import FilterHeader from '@/app/(protected)/markets/components/filter-header';
import { cleanup, fireEvent, render, screen } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

const navigationMocks = vi.hoisted(() => ({
  pathname: '/markets',
  push: vi.fn(),
  replace: vi.fn(),
  searchParams: new URLSearchParams(),
}));
const filterDebounceMs = 350;

vi.mock('next/navigation', () => ({
  usePathname: () => navigationMocks.pathname,
  useRouter: () => ({
    push: navigationMocks.push,
    replace: navigationMocks.replace,
  }),
  useSearchParams: () => navigationMocks.searchParams,
}));

function renderFilterHeader() {
  return render(
    <FilterHeader
      markets={[
        { id: '1', name: 'Mercadona' },
        { id: '2', name: 'Hiperdino' },
      ]}
    />,
  );
}

describe('market filter header component', () => {
  beforeEach(() => {
    navigationMocks.pathname = '/markets';
    navigationMocks.push.mockReset();
    navigationMocks.replace.mockReset();
    navigationMocks.searchParams = new URLSearchParams();
    vi.useFakeTimers();
  });

  afterEach(() => {
    cleanup();
    vi.useRealTimers();
  });

  it('writes product name search to URL query', () => {
    renderFilterHeader();

    fireEvent.change(screen.getByPlaceholderText('Search products...'), {
      target: { value: 'milk' },
    });
    vi.advanceTimersByTime(filterDebounceMs);

    expect(navigationMocks.replace).toHaveBeenLastCalledWith(
      '/markets?query=milk',
    );
  });

  it('writes brand search to URL query', () => {
    renderFilterHeader();

    fireEvent.change(screen.getByPlaceholderText('Brand...'), {
      target: { value: 'Pascual' },
    });
    vi.advanceTimersByTime(filterDebounceMs);

    expect(navigationMocks.replace).toHaveBeenLastCalledWith(
      '/markets?brand=Pascual',
    );
  });

  it('writes market selection to URL query', () => {
    renderFilterHeader();

    fireEvent.change(screen.getAllByRole('combobox')[0], {
      target: { value: '1' },
    });

    expect(navigationMocks.replace).toHaveBeenLastCalledWith(
      '/markets?marketId=1',
    );
  });

  it('reflects product name from URL navigation', () => {
    const { rerender } = renderFilterHeader();
    navigationMocks.searchParams = new URLSearchParams('query=olive%20oil');

    rerender(
      <FilterHeader
        markets={[
          { id: '1', name: 'Mercadona' },
          { id: '2', name: 'Hiperdino' },
        ]}
      />,
    );

    expect(screen.getByPlaceholderText('Search products...')).toHaveValue(
      'olive oil',
    );
  });

  it('reflects brand name from URL navigation', () => {
    const { rerender } = renderFilterHeader();
    navigationMocks.searchParams = new URLSearchParams('brand=Pascual');

    rerender(
      <FilterHeader
        markets={[
          { id: '1', name: 'Mercadona' },
          { id: '2', name: 'Hiperdino' },
        ]}
      />,
    );

    expect(screen.getByPlaceholderText('Brand...')).toHaveValue('Pascual');
  });
});
