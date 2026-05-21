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
  render(<FilterHeader marketNames={['Mercadona', 'Hiperdino']} />);
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
      '/markets?name_segment=milk',
    );
  });

  it('writes brand search to URL query', () => {
    renderFilterHeader();

    fireEvent.change(screen.getByPlaceholderText('Brand...'), {
      target: { value: 'Pascual' },
    });
    vi.advanceTimersByTime(filterDebounceMs);

    expect(navigationMocks.replace).toHaveBeenLastCalledWith(
      '/markets?brand_name=Pascual',
    );
  });

  it('writes market selection to URL query', () => {
    renderFilterHeader();

    fireEvent.change(screen.getByRole('combobox'), {
      target: { value: 'Mercadona' },
    });

    expect(navigationMocks.replace).toHaveBeenLastCalledWith(
      '/markets?market_name=Mercadona',
    );
  });
});
