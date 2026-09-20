import { expect, it } from 'vitest';

import { describeIfRest, registerAndLogin } from './rest-test-client';

describeIfRest('auth REST integration', () => {
  it('returns an HttpOnly session token after registration and login', async () => {
    const response = await registerAndLogin();

    expect(response.httpOnly).toBe(true);
  });

  it('returns expiration after registration and login', async () => {
    const response = await registerAndLogin();

    expect(response.expirationInUtc).not.toBe('');
  });
});
