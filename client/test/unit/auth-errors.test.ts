import { describe, expect, it } from 'vitest';

import { GrpcErrorMessage } from '@/lib/auth-errors';

describe('GrpcErrorMessage', () => {
  it('uses grpc details when present', () => {
    expect(
      new GrpcErrorMessage({ details: 'Invalid credentials.' }, 'Fallback.')
        .message,
    ).toBe('Invalid credentials.');
  });

  it('uses fallback when details are missing or empty', () => {
    expect(new GrpcErrorMessage({}, 'Fallback.').message).toBe('Fallback.');
    expect(new GrpcErrorMessage({ details: '' }, 'Fallback.').message).toBe(
      'Fallback.',
    );
  });

  it('uses fallback for non-object errors', () => {
    expect(new GrpcErrorMessage('failed', 'Fallback.').message).toBe(
      'Fallback.',
    );
  });
});
