/* eslint-disable @typescript-eslint/no-magic-numbers */
import { afterEach, describe, expect, it, vi } from 'vitest';

import { LoginResult } from '@/lib/auth-domain';

const NOW = new Date('2026-05-26T12:00:00.000Z');

afterEach(() => {
  vi.useRealTimers();
});

describe('LoginResult constructor', () => {
  it('should store the token', () => {
    // Arrange
    const expiration = new Date('2026-05-26T12:01:00.000Z');

    // Act
    const result = new LoginResult('my-token', expiration);

    // Assert
    expect(result.token).toBe('my-token');
  });

  it('should store the expiration date', () => {
    // Arrange
    const expiration = new Date('2026-05-26T12:01:00.000Z');

    // Act
    const result = new LoginResult('my-token', expiration);

    // Assert
    expect(result.expirationInUtc).toBe(expiration);
  });
});

describe('LoginResult isExpired', () => {
  it('should return false if expiration is in the future', () => {
    // Arrange
    vi.useFakeTimers();
    vi.setSystemTime(NOW);
    const futureDate = new Date('2026-05-26T12:01:00.000Z');
    const result = new LoginResult('token', futureDate);

    // Act
    const isExpired = result.isExpired();

    // Assert
    expect(isExpired).toBe(false);
  });

  it('should return true if expiration is in the past', () => {
    // Arrange
    vi.useFakeTimers();
    vi.setSystemTime(NOW);
    const pastDate = new Date('2026-05-26T11:59:00.000Z');
    const result = new LoginResult('token', pastDate);

    // Act
    const isExpired = result.isExpired();

    // Assert
    expect(isExpired).toBe(true);
  });
});
