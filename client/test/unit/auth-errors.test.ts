import { describe, expect, it } from 'vitest';

import { getAuthErrorMessage } from '@/lib/auth-errors';

describe('getAuthErrorMessage', () => {
  it('uses the error message when present', () => {
    expect(
      getAuthErrorMessage(new Error('Invalid credentials.'), 'Fallback.'),
    ).toBe('Invalid credentials.');
  });

  it('uses fallback when the error message is empty', () => {
    expect(getAuthErrorMessage(new Error(''), 'Fallback.')).toBe('Fallback.');
  });

  it('uses fallback for non-object errors', () => {
    expect(getAuthErrorMessage('failed', 'Fallback.')).toBe('Fallback.');
  });
});
