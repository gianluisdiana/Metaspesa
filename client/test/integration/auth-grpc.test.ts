import { expect, it } from 'vitest';

import { describeIfGrpc, registerAndLogin } from './grpc-test-client';

describeIfGrpc('auth gRPC integration', () => {
  it('returns token after registration and login', async () => {
    const response = await registerAndLogin();

    expect(response.token).not.toBe('');
  });

  it('returns expiration after registration and login', async () => {
    const response = await registerAndLogin();

    expect(response.expirationInUtc).not.toBe('');
  });
});
