import { describe, expect, it } from 'vitest';

import { GrpcAuthMapper } from '@/infrastructure/grpc-auth-mapper';

describe('GrpcAuthMapper', () => {
  const mapper = new GrpcAuthMapper();

  it('maps missing login responses to empty values', () => {
    expect(mapper.mapLoginResult()).toEqual({
      expirationInUtc: '',
      token: '',
    });
  });

  it('maps login responses', () => {
    expect(
      mapper.mapLoginResult({
        expirationInUtc: '2026-05-18T22:30:00Z',
        token: 'token',
      }),
    ).toEqual({
      expirationInUtc: '2026-05-18T22:30:00Z',
      token: 'token',
    });
  });
});
