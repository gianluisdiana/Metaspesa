/* eslint-disable @typescript-eslint/no-magic-numbers */
import { describe, expect, it } from 'vitest';

import { Product } from '@/lib/domain';

describe('Product constructor', () => {
  it('should create a product with the given name', () => {
    // Act
    const product = new Product('Milk');

    // Assert
    expect(product.name).toBe('Milk');
  });

  it('should create a product with the given quantity', () => {
    // Act
    const product = new Product('Milk', '2 liters');

    // Assert
    expect(product.quantity).toBe('2 liters');
  });

  it('should not have a quantity if not provided', () => {
    // Act
    const product = new Product('Milk');

    // Assert
    expect(product.quantity).toBeUndefined();
  });

  it('should create a product with the given price', () => {
    // Arrange
    const price = 1.99;

    // Act
    const product = new Product('Milk', undefined, price);

    // Assert
    expect(product.price).toBe(price);
  });

  it('should not have a price if not provided', () => {
    // Act
    const product = new Product('Milk');

    // Assert
    expect(product.price).toBeUndefined();
  });

  it('should be checked by default', () => {
    // Act
    const product = new Product('Milk');

    // Assert
    expect(product.checked).toBe(true);
  });

  it('should create a product with the given checked status', () => {
    // Act
    const product = new Product('Milk', undefined, undefined, false);

    // Assert
    expect(product.checked).toBe(false);
  });
});

describe('Product hasValidQuantity', () => {
  it('should return true if quantity is undefined', () => {
    // Arrange
    const product = new Product('Milk');

    // Act
    const isValid = product.hasValidQuantity();

    // Assert
    expect(isValid).toBe(true);
  });

  it('should return true if quantity is a non-empty string', () => {
    // Arrange
    const product = new Product('Milk', '2 liters');

    // Act
    const isValid = product.hasValidQuantity();

    // Assert
    expect(isValid).toBe(true);
  });

  it('should return true if quantity is an empty string', () => {
    // Arrange
    const product = new Product('Milk', '');

    // Act
    const isValid = product.hasValidQuantity();

    // Assert
    expect(isValid).toBe(true);
  });

  it('should return true if quantity is a string with only whitespace', () => {
    // Arrange
    const product = new Product('Milk', '   ');

    // Act
    const isValid = product.hasValidQuantity();

    // Assert
    expect(isValid).toBe(true);
  });

  it('should return false if quantity is a string longer than 50 characters', () => {
    // Arrange
    const longQuantity = 'a'.repeat(51);
    const product = new Product('Milk', longQuantity);

    // Act
    const isValid = product.hasValidQuantity();

    // Assert
    expect(isValid).toBe(false);
  });
});

describe('Product hasValidPrice', () => {
  it('should return true if price is undefined', () => {
    // Arrange
    const product = new Product('Milk');

    // Act
    const isValid = product.hasValidPrice();

    // Assert
    expect(isValid).toBe(true);
  });

  it('should return true if price is a non-negative number', () => {
    // Arrange
    const product = new Product('Milk', undefined, 3.4);

    // Act
    const isValid = product.hasValidPrice();

    // Assert
    expect(isValid).toBe(true);
  });

  it('should return true if price is zero', () => {
    // Arrange
    const product = new Product('Milk', undefined, 0);

    // Act
    const isValid = product.hasValidPrice();

    // Assert
    expect(isValid).toBe(true);
  });

  it('should return false if price is a negative number', () => {
    // Arrange
    const product = new Product('Milk', undefined, -5.5);

    // Act
    const isValid = product.hasValidPrice();

    // Assert
    expect(isValid).toBe(false);
  });
});

describe('Product canBeCounted', () => {
  it('should not be counted if is not checked', () => {
    // Arrange
    const product = new Product('Milk', undefined, 1.99, false);

    // Act
    const canBeCounted = product.canBeCounted();

    // Assert
    expect(canBeCounted).toBe(false);
  });

  it('should not be counted if price is undefined', () => {
    // Arrange
    const product = new Product('Milk');

    // Act
    const canBeCounted = product.canBeCounted();

    // Assert
    expect(canBeCounted).toBe(false);
  });

  it('should not be counted if price is invalid', () => {
    // Arrange
    const product = new Product('Milk', undefined, -1.99);

    // Act
    const canBeCounted = product.canBeCounted();

    // Assert
    expect(canBeCounted).toBe(false);
  });

  it('should be counted if is checked and has a valid price', () => {
    // Arrange
    const product = new Product('Milk', undefined, 1.99);

    // Act
    const canBeCounted = product.canBeCounted();

    // Assert
    expect(canBeCounted).toBe(true);
  });
});
