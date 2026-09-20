/* @vitest-environment jsdom */
import AddToListModal from '@/app/(protected)/markets/components/add-to-list-modal';
import { cleanup, fireEvent, render, screen } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';

const namedListId = 7;

function renderModal({
  onCreateList = vi.fn(),
  onSelectList = vi.fn(),
  shoppingListSummaries = [
    { id: 9, isTemporary: true },
    { id: namedListId, isTemporary: false, name: 'Weekly' },
  ],
} = {}) {
  render(
    <AddToListModal
      isOpen
      productName="Bread"
      shoppingListSummaries={shoppingListSummaries}
      onClose={() => undefined}
      onCreateList={onCreateList}
      onSelectList={onSelectList}
    />,
  );

  return { onCreateList, onSelectList };
}

describe('add to list modal component', () => {
  afterEach(cleanup);

  it('renders product name being added', () => {
    renderModal();

    expect(screen.getByText('Bread')).toBeInTheDocument();
  });

  it('selects temporary list by default', () => {
    renderModal();

    expect(screen.getByLabelText('Temporary List')).toBeChecked();
  });

  it('renders named shopping list option', () => {
    renderModal();

    expect(screen.getByLabelText('Weekly')).toBeInTheDocument();
  });

  it('submits selected named shopping list', () => {
    const { onSelectList } = renderModal();

    fireEvent.click(screen.getByLabelText('Weekly'));
    fireEvent.click(screen.getByText('Add to list'));

    expect(onSelectList).toHaveBeenCalledWith(namedListId);
  });

  it('creates shopping list when no choices exist', () => {
    const { onCreateList } = renderModal({ shoppingListSummaries: [] });

    fireEvent.click(screen.getByText('Create shopping list'));

    expect(onCreateList).toHaveBeenCalledOnce();
  });
});
