import { afterEach, describe, expect, it, vi } from 'vitest';

import { getRestApiUrl } from '@/lib/rest-api-url';

afterEach(() => {
  vi.unstubAllEnvs();
  vi.unstubAllGlobals();
});

describe('REST API URL', () => {
  it('uses the internal API address during server rendering', () => {
    vi.stubEnv('REST_API_URL', 'http://server:8080/api/v1');
    vi.stubEnv('NEXT_PUBLIC_REST_API_URL', 'http://localhost:4001/api/v1');

    expect(getRestApiUrl()).toBe('http://server:8080/api/v1');
  });

  it('uses the public API address in the browser', () => {
    vi.stubGlobal('window', {});
    vi.stubEnv('REST_API_URL', 'http://server:8080/api/v1');
    vi.stubEnv('NEXT_PUBLIC_REST_API_URL', 'http://localhost:4001/api/v1');

    expect(getRestApiUrl()).toBe('http://localhost:4001/api/v1');
  });
});
