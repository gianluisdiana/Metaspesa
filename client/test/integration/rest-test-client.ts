import { randomUUID } from 'node:crypto';

import { describe } from 'vitest';

export const restApiUrl = process.env.REST_API_URL;
export const describeIfRest = restApiUrl ? describe : describe.skip;
const password = 'SecurePass1!';

export async function registerAndLogin(): Promise<{
  expirationInUtc: string;
  httpOnly: boolean;
  token: string;
}> {
  const credentials = {
    password,
    username: `client_it_${randomUUID().replaceAll('-', '_')}`,
  };

  const registration = await fetch(`${restApiUrl}/auth/registrations`, {
    body: JSON.stringify(credentials),
    headers: {
      'Content-Type': 'application/json',
      Origin: 'http://localhost:3000',
    },
    method: 'POST',
  });
  if (!registration.ok) {
    throw new Error(`Registration failed with ${registration.status}.`);
  }

  const session = await fetch(`${restApiUrl}/auth/sessions`, {
    body: JSON.stringify(credentials),
    headers: {
      'Content-Type': 'application/json',
      Origin: 'http://localhost:3000',
    },
    method: 'POST',
  });
  if (!session.ok) {
    throw new Error(`Login failed with ${session.status}.`);
  }
  const cookie = session.headers.get('set-cookie');
  const cookieParts = cookie?.split(';') ?? [];
  const [sessionPart] = cookieParts;
  const token = sessionPart?.startsWith('metaspesa_session=')
    ? sessionPart.slice('metaspesa_session='.length)
    : undefined;
  const expirationInUtc = cookieParts
    .find(part => part.trimStart().toLowerCase().startsWith('expires='))
    ?.trimStart()
    .slice('expires='.length);
  const httpOnly = cookieParts.some(
    part => part.trim().toLowerCase() === 'httponly',
  );
  if (!token || !expirationInUtc) {
    throw new Error('Login did not return the session cookie.');
  }
  return { expirationInUtc, httpOnly, token };
}
