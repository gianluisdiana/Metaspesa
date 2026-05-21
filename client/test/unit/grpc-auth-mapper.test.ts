import { describe, expect, it } from 'vitest';

import { GrpcAuthMapper } from '@/infrastructure/grpc-auth-mapper';

describe('GrpcAuthMapper', () => {
  const mapper = new GrpcAuthMapper();

  it('fails on missing login responses', () => {
    expect(() => mapper.mapLoginResult()).toThrow(
      'Malformed gRPC response: LoginResponse',
    );
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
