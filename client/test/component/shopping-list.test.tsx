/* @vitest-environment jsdom */
import { ToastProvider } from '@/app/(protected)/components/toast-provider';
import ShoppingListContainer from '@/app/(protected)/shopping/components/shopping-list-container';
import { cleanup, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';

const httpStatus = { conflict: 409, created: 201, ok: 200 } as const;
const listId = '00000000-0000-7000-8000-000000000007';
const temporaryListId = '00000000-0000-7000-8000-000000000009';
const milkFormatId = '00000000-0000-7000-8000-000000000001';
const breadFormatId = '00000000-0000-7000-8000-000000000002';

const navigationMocks = vi.hoisted(() => ({ push: vi.fn() }));

vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: navigationMocks.push }),
}));

function jsonResponse(body: unknown, status: number = httpStatus.ok): Response {
  return new Response(JSON.stringify(body), {
    headers: { 'Content-Type': 'application/json' },
    status,
  });
}

function renderShoppingList() {
  render(
    <ToastProvider>
      <ShoppingListContainer
        initialSelectedListId={listId}
        initialShoppingList={{
          id: listId,
          name: 'Groceries',
          products: [
            {
              checked: false,
              name: 'Milk',
              price: 1.25,
              productFormatUid: milkFormatId,
              quantity: '1 l',
            },
            {
              checked: true,
              name: 'Bread',
              price: 2.35,
              productFormatUid: breadFormatId,
            },
          ],
        }}
        initialShoppingListSummaries={[
          { id: listId, isTemporary: false, name: 'Groceries' },
        ]}
      />
    </ToastProvider>,
  );
}

function renderUserWithoutShoppingLists() {
  render(
    <ToastProvider>
      <ShoppingListContainer
        initialShoppingList={{ products: [] }}
        initialShoppingListSummaries={[]}
      />
    </ToastProvider>,
  );
}

describe('shopping list component', () => {
  afterEach(() => {
    cleanup();
    vi.unstubAllGlobals();
    vi.clearAllMocks();
  });

  it('renders selected shopping list and totals', () => {
    renderShoppingList();

    expect(screen.getByRole('heading', { name: 'Groceries' })).toBeVisible();
    expect(screen.getByText('2 items')).toBeVisible();
    expect(screen.getByText('$3.60')).toBeVisible();
  });

  it('guides users without shopping lists', () => {
    renderUserWithoutShoppingLists();

    expect(
      screen.getByRole('heading', { name: 'No shopping lists yet' }),
    ).toBeVisible();
  });

  it('creates temporary list and navigates by stable ID', async () => {
    const fetcher = vi
      .fn()
      .mockResolvedValueOnce(
        jsonResponse({ id: temporaryListId }, httpStatus.created),
      )
      .mockResolvedValueOnce(
        jsonResponse({ id: temporaryListId, isTemporary: true, items: [] }),
      )
      .mockResolvedValueOnce(
        jsonResponse({ items: [{ id: temporaryListId, isTemporary: true }] }),
      );
    vi.stubGlobal('fetch', fetcher);
    const user = userEvent.setup();
    renderUserWithoutShoppingLists();

    await user.click(
      screen.getByRole('button', { name: 'Create shopping list' }),
    );

    expect(
      await screen.findByRole('heading', { name: 'Temporary List' }),
    ).toBeVisible();
    expect(navigationMocks.push).toHaveBeenCalledWith(
      `/shopping?listId=${temporaryListId}`,
    );
  });

  it('offers naming when temporary list already exists', async () => {
    const fetcher = vi
      .fn()
      .mockResolvedValueOnce(
        jsonResponse(
          {
            code: 'ShoppingList.Temporary.AlreadyExists',
            title: 'Temporary shopping list already exists',
          },
          httpStatus.conflict,
        ),
      )
      .mockResolvedValueOnce(
        jsonResponse({ items: [{ id: temporaryListId, isTemporary: true }] }),
      );
    vi.stubGlobal('fetch', fetcher);
    const user = userEvent.setup();
    renderShoppingList();

    await user.click(
      screen.getByRole('button', { name: 'Create shopping list' }),
    );

    expect(
      await screen.findByText(
        'Temporary list already exists. Name it and create a new one?',
      ),
    ).toBeVisible();
  });
});
