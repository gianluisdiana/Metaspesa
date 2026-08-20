/* @vitest-environment jsdom */
import { ToastProvider } from '@/app/(protected)/components/toast-provider';
import ShoppingListContainer from '@/app/(protected)/shopping/components/shopping-list-container';
import { cleanup, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';

const navigationMocks = vi.hoisted(() => ({
  push: vi.fn(),
  replace: vi.fn(),
}));

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: navigationMocks.push,
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
              productFormatUid: 1,
              quantity: '1 l',
            },
            {
              checked: true,
              name: 'Bread',
              price: 2.35,
              productFormatUid: 2,
            },
          ],
        }}
        initialShoppingListSummaries={[{ name: 'Groceries' }]}
      />
    </ToastProvider>,
  );
}

function shoppingListResponse(body: unknown, ok = true) {
  return {
    json: () => Promise.resolve(body),
    ok,
  };
}

function temporaryListConflictResponse() {
  return shoppingListResponse({
    message: 'Temporary list already exists. Name it and create a new one?',
    requiresTemporaryListName: true,
    shoppingList: { name: undefined, products: [] },
    shoppingListSummaries: [{ name: undefined }],
  });
}

function temporaryListCreatedResponse() {
  return shoppingListResponse({
    message: 'Temporary list created.',
    shoppingList: { name: undefined, products: [] },
    shoppingListSummaries: [{ name: 'Groceries' }, { name: undefined }],
  });
}

describe('shopping list component', () => {
  afterEach(() => {
    cleanup();
    vi.unstubAllGlobals();
    vi.clearAllMocks();
  });

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

  it('shows a naming dialog when creating a temporary list that already exists', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(temporaryListConflictResponse()),
    );
    const user = userEvent.setup();
    renderShoppingList();

    await user.click(
      screen.getByRole('button', {
        name: 'Create shopping list',
      }),
    );

    expect(
      await screen.findByText(
        'Temporary list already exists. Name it and create a new one?',
      ),
    ).toBeVisible();
  });

  it('navigates to the new temporary list after naming the existing temporary list', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(temporaryListConflictResponse())
      .mockResolvedValueOnce(temporaryListCreatedResponse());
    vi.stubGlobal('fetch', fetchMock);
    const user = userEvent.setup();
    renderShoppingList();

    await user.click(
      screen.getByRole('button', {
        name: 'Create shopping list',
      }),
    );
    await user.type(await screen.findByLabelText('List name'), 'Groceries');
    await user.click(screen.getByRole('button', { name: 'Create' }));

    expect(navigationMocks.push).toHaveBeenCalledWith('/shopping');
  });

  it('disables create action until the temporary list name has text', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(temporaryListConflictResponse()),
    );
    const user = userEvent.setup();
    renderShoppingList();

    await user.click(
      screen.getByRole('button', {
        name: 'Create shopping list',
      }),
    );

    expect(
      await screen.findByRole('button', { name: 'Create' }),
    ).toBeDisabled();
  });

  it('keeps naming dialog open when naming the temporary list fails', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(temporaryListConflictResponse())
      .mockResolvedValueOnce(
        shoppingListResponse(
          {
            message: 'Could not create a temporary list.',
            shoppingList: { name: undefined, products: [] },
            shoppingListSummaries: [{ name: undefined }],
          },
          false,
        ),
      );
    vi.stubGlobal('fetch', fetchMock);
    const user = userEvent.setup();
    renderShoppingList();

    await user.click(
      screen.getByRole('button', {
        name: 'Create shopping list',
      }),
    );
    await user.type(await screen.findByLabelText('List name'), 'Groceries');
    await user.click(screen.getByRole('button', { name: 'Create' }));

    expect(
      await screen.findByText(
        'Temporary list already exists. Name it and create a new one?',
      ),
    ).toBeVisible();
  });
});
