/* eslint-disable @typescript-eslint/no-magic-numbers */
import { describe, expect, it } from 'vitest';

import {
  ShoppingListTabsViewModel,
  ShoppingListViewModel,
  ShoppingPriceViewModel,
} from '@/lib/shopping-list-view-model';

describe('ShoppingPriceViewModel', () => {
  it('formats missing prices as a placeholder', () => {
    expect(new ShoppingPriceViewModel().displayValue).toBe('-');
  });

  it('formats prices with two decimals', () => {
    expect(new ShoppingPriceViewModel(1.2).displayValue).toBe('$1.20');
  });

  it('rounds prices to two decimals', () => {
    expect(new ShoppingPriceViewModel(1.235).displayValue).toBe('$1.24');
  });
});

describe('ShoppingListViewModel', () => {
  it('creates an empty temporary list view model', () => {
    const viewModel = new ShoppingListViewModel({
      products: [],
    });

    expect(viewModel.listName).toBe('Temporary List');
    expect(viewModel.itemCountLabel).toBe('0 items');
    expect(viewModel.hasItems).toBe(false);
    expect(viewModel.progress.checkedCount).toBe(0);
    expect(viewModel.progress.label).toBe('0 of 0 items found');
    expect(viewModel.progress.percentage).toBe(0);
    expect(viewModel.progress.totalCount).toBe(0);
    expect(viewModel.checkedTotal).toBe('$0.00');
    expect(viewModel.estimatedTotal).toBe('$0.00');
  });

  it('formats totals, progress, and checked/unchecked sections', () => {
    const viewModel = new ShoppingListViewModel({
      name: 'Groceries',
      products: [
        { checked: false, name: 'Milk', price: 1.2, quantity: '1 liter' },
        { checked: true, name: 'Bread', price: 2.345 },
        { checked: true, name: 'Eggs' },
      ],
    });

    expect(viewModel.listName).toBe('Groceries');
    expect(viewModel.itemCount).toBe(3);
    expect(viewModel.itemCountLabel).toBe('3 items');
    expect(viewModel.progress.label).toBe('2 of 3 items found');
    expect(viewModel.progress.percentage).toBeCloseTo(66.666);
    expect(viewModel.checkedTotal).toBe('$2.35');
    expect(viewModel.estimatedTotal).toBe('$3.55');
    expect(viewModel.uncheckedSections).toHaveLength(1);
    expect(viewModel.uncheckedSections[0].label).toBe('To buy');
    expect(viewModel.uncheckedSections[0].items).toHaveLength(1);
    expect(viewModel.uncheckedSections[0].items[0].badge).toEqual({
      colorClass: 'bg-tertiary-container/30 text-on-tertiary-container',
      label: 'Item',
    });
    expect(viewModel.uncheckedSections[0].items[0].categorySection).toBe(
      'To buy',
    );
    expect(viewModel.uncheckedSections[0].items[0].id).toBe('Milk-0');
    expect(viewModel.uncheckedSections[0].items[0].lowStock).toBe(false);
    expect(viewModel.uncheckedSections[0].items[0].name).toBe('Milk');
    expect(viewModel.uncheckedSections[0].items[0].price).toBe('$1.20');
    expect(viewModel.uncheckedSections[0].items[0].qty).toBe('1 liter');
    expect(viewModel.checkedItems).toHaveLength(2);
    expect(viewModel.checkedItems[0].id).toBe('Bread-0');
    expect(viewModel.checkedItems[0].name).toBe('Bread');
    expect(viewModel.checkedItems[0].price).toBe('$2.35');
    expect(viewModel.checkedItems[1].id).toBe('Eggs-1');
    expect(viewModel.checkedItems[1].name).toBe('Eggs');
    expect(viewModel.checkedItems[1].price).toBe('-');
  });

  it('uses a singular item count label', () => {
    const viewModel = new ShoppingListViewModel({
      products: [{ checked: false, name: 'Milk' }],
    });

    expect(viewModel.itemCountLabel).toBe('1 item');
  });
});

describe('ShoppingListTabsViewModel', () => {
  it('creates one tab per shopping list summary', () => {
    const viewModel = new ShoppingListTabsViewModel([
      {},
      { name: 'Groceries' },
    ]);

    expect(viewModel.tabs).toHaveLength(2);
  });

  it('formats the temporary list tab label', () => {
    const viewModel = new ShoppingListTabsViewModel([{}]);

    expect(viewModel.tabs[0].label).toBe('Temporary List');
  });

  it('formats named list tab label', () => {
    const viewModel = new ShoppingListTabsViewModel([{ name: 'Groceries' }]);

    expect(viewModel.tabs[0].label).toBe('Groceries');
  });

  it('marks the selected named tab active', () => {
    const viewModel = new ShoppingListTabsViewModel(
      [{ name: 'Groceries' }],
      'Groceries',
    );

    expect(viewModel.tabs[0].active).toBe(true);
  });

  it('marks the temporary tab active when no list name is selected', () => {
    const viewModel = new ShoppingListTabsViewModel([{}]);

    expect(viewModel.tabs[0].active).toBe(true);
  });

  it('uses the current shopping list as a fallback tab', () => {
    const viewModel = new ShoppingListTabsViewModel([], undefined, {
      products: [],
    });

    expect(viewModel.tabs[0].label).toBe('Temporary List');
  });

  it('sorts tabs with temporary list first and then alphabetically', () => {
    const viewModel = new ShoppingListTabsViewModel(
      [{ name: 'Vegetables' }, {}, { name: 'Fruits' }, { name: 'Dairy' }],
      'Fruits',
    );

    expect(viewModel.tabs).toHaveLength(4);
    expect(viewModel.tabs[0].label).toBe('Temporary List');
    expect(viewModel.tabs[1].label).toBe('Dairy');
    expect(viewModel.tabs[2].label).toBe('Fruits');
    expect(viewModel.tabs[3].label).toBe('Vegetables');
  });
});
