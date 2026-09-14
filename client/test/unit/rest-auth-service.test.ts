import { describe, expect, it, vi } from 'vitest';

import RestAuthService from '@/infrastructure/rest-auth-service';

describe('RestAuthService', () => {
  it('calls default fetch with global object as receiver', async () => {
    const fetcher = vi.fn(function (
      this: typeof globalThis,
    ): Promise<Response> {
      if (this !== globalThis) {
        throw new TypeError('Invalid fetch receiver.');
      }
      return Promise.resolve(new Response(null, { status: 204 }));
    });
    vi.stubGlobal('fetch', fetcher);

    try {
      const service = new RestAuthService('http://identity/api/v1');

      await service.login({ password: 'SecurePass1!', username: 'estela' });
    } finally {
      vi.unstubAllGlobals();
    }
  });

  it('posts browser login credentials to the session endpoint', async () => {
    const fetcher = vi
      .fn()
      .mockResolvedValue(new Response(null, { status: 204 }));
    const service = new RestAuthService('http://identity/api/v1', fetcher);

    await service.login({ password: 'SecurePass1!', username: 'Café' });

    expect(fetcher).toHaveBeenCalledWith(
      'http://identity/api/v1/auth/sessions',
      expect.objectContaining({
        body: JSON.stringify({ password: 'SecurePass1!', username: 'Café' }),
        credentials: 'include',
        method: 'POST',
      }),
    );
  });

  it('posts registration credentials to the registration endpoint', async () => {
    const fetcher = vi
      .fn()
      .mockResolvedValue(new Response(null, { status: 201 }));
    const service = new RestAuthService('http://identity/api/v1', fetcher);

    await service.register({ password: 'SecurePass1!', username: 'estela' });

    expect(fetcher).toHaveBeenCalledWith(
      'http://identity/api/v1/auth/registrations',
      expect.objectContaining({ method: 'POST' }),
    );
  });

  it('uses Problem Details title when login fails', async () => {
    const fetcher = vi.fn().mockResolvedValue(
      Response.json(
        { title: 'Invalid credentials' },
        {
          headers: { 'Content-Type': 'application/problem+json' },
          status: 401,
        },
      ),
    );
    const service = new RestAuthService('http://identity/api/v1', fetcher);

    await expect(
      service.login({ password: 'wrong', username: 'estela' }),
    ).rejects.toThrow('Invalid credentials');
  });

  it('uses stable fallback when registration error is malformed', async () => {
    const fetcher = vi
      .fn()
      .mockResolvedValue(new Response('not-json', { status: 500 }));
    const service = new RestAuthService('http://identity/api/v1', fetcher);

    await expect(
      service.register({ password: 'SecurePass1!', username: 'estela' }),
    ).rejects.toThrow('Registration failed.');
  });
});
