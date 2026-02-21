/* eslint-disable @typescript-eslint/no-magic-numbers */
import { describe, expect, it } from 'vitest';

import { Product, ShoppingList } from '@/lib/domain';

describe('ShoppingList calculateTotal', () => {
  it('should calculate 0 if there are no products', () => {
    // Arrange
    const shoppingList = new ShoppingList([], 'Test List');

    // Act
    const total = shoppingList.calculateTotal();

    // Assert
    expect(total).toBe(0);
  });

  it('should calculate sum of product prices', () => {
    // Arrange
    const products = [
      new Product('Milk', undefined, 1),
      new Product('Bread', undefined, 2),
      new Product('Eggs', undefined, 3),
    ];
    const shoppingList = new ShoppingList(products, 'Groceries');

    // Act
    const total = shoppingList.calculateTotal();

    // Assert
    expect(total).toBe(6);
  });

  it('should ignore unchecked products', () => {
    // Arrange
    const products = [
      new Product('Milk', undefined, 1, false),
      new Product('Bread', undefined, 2),
      new Product('Eggs', undefined, 3),
    ];
    const shoppingList = new ShoppingList(products, 'Groceries');

    // Act
    const total = shoppingList.calculateTotal();

    // Assert
    expect(total).toBe(5);
  });

  it('should ignore products without price', () => {
    // Arrange
    const products = [
      new Product('Milk', undefined, 1),
      new Product('Bread'),
      new Product('Eggs', undefined, 3),
    ];
    const shoppingList = new ShoppingList(products, 'Groceries');

    // Act
    const total = shoppingList.calculateTotal();

    // Assert
    expect(total).toBe(4);
  });

  it('should round total to two decimal places', () => {
    // Arrange
    const products = [
      new Product('Milk', undefined, 1.234),
      new Product('Bread', undefined, 2.345),
    ];
    const shoppingList = new ShoppingList(products, 'Groceries');

    // Act
    const total = shoppingList.calculateTotal();

    // Assert
    expect(total).toBe(3.58);
  });
});

describe('ShoppingList contains', () => {
  it('should not contain product if list is empty', () => {
    // Arrange
    const shoppingList = new ShoppingList([], 'Test List');
    const product = new Product('Milk');

    // Act
    const contains = shoppingList.contains(product);

    // Assert
    expect(contains).toBe(false);
  });

  it('should contain product if there\'s one with the same name', () => {
    // Arrange
    const product = new Product('Milk');
    const shoppingList = new ShoppingList([product], 'Groceries');

    // Act
    const contains = shoppingList.contains(product);

    // Assert
    expect(contains).toBe(true);
  });

  it('should contain product if there\'s one with the same name but different properties', () => {
    // Arrange
    const productInList = new Product('Milk', '1 liter', 1.5, false);
    const shoppingList = new ShoppingList([productInList], 'Groceries');
    const productToCheck = new Product('Milk', '2 liters', 3, true);

    // Act
    const contains = shoppingList.contains(productToCheck);

    // Assert
    expect(contains).toBe(true);
  });

  it('should not contain product if there isn\'t one with the same name', () => {
    // Arrange
    const productInList = new Product('Milk');
    const shoppingList = new ShoppingList([productInList], 'Groceries');
    const productToCheck = new Product('Bread');

    // Act
    const contains = shoppingList.contains(productToCheck);

    // Assert
    expect(contains).toBe(false);
  });
});