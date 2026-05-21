/* eslint-disable @typescript-eslint/no-magic-numbers */
import { describe, expect, it } from 'vitest';

import { LoginResult } from '@/lib/auth-domain';

describe('LoginResult constructor', () => {
  it('should store the token', () => {
    // Arrange
    const expiration = new Date(Date.now() + 60000);

    // Act
    const result = new LoginResult('my-token', expiration);

    // Assert
    expect(result.token).toBe('my-token');
  });

  it('should store the expiration date', () => {
    // Arrange
    const expiration = new Date(Date.now() + 60000);

    // Act
    const result = new LoginResult('my-token', expiration);

    // Assert
    expect(result.expirationInUtc).toBe(expiration);
  });
});

describe('LoginResult isExpired', () => {
  it('should return false if expiration is in the future', () => {
    // Arrange
    const futureDate = new Date(Date.now() + 60000);
    const result = new LoginResult('token', futureDate);

    // Act & Assert
    expect(result.isExpired()).toBe(false);
  });

  it('should return true if expiration is in the past', () => {
    // Arrange
    const pastDate = new Date(Date.now() - 60000);
    const result = new LoginResult('token', pastDate);

    // Act & Assert
    expect(result.isExpired()).toBe(true);
  });
});
