'use server';

import { redirect } from 'next/navigation';
import { cookies } from 'next/headers';

import GrpcAuthService from '@/infrastructure/grpc-auth-service';
import { Credentials } from '@/lib/auth-domain';

export type LoginState = { error: string } | null;

export async function loginAction(
  _: LoginState,
  formData: FormData,
): Promise<LoginState> {
  const username = (formData.get('username') as string) ?? '';
  const password = (formData.get('password') as string) ?? '';

  const credentials = new Credentials(username, password);
  if (!credentials.hasValidUsername()) {
    return { error: 'Username cannot be empty.' };
  }
  const service = new GrpcAuthService();
  let token: string;
  let expirationInUtc: string;
  try {
    const result = await service.login({ password, username });
    token = result.token;
    expirationInUtc = result.expirationInUtc;
  } catch (err) {
    return { error: getGrpcErrorMessage(err) };
  }

  const cookieStore = await cookies();
  cookieStore.set('auth_token', token, {
    httpOnly: true,
    sameSite: 'lax',
    secure: process.env.NODE_ENV === 'production',
    expires: new Date(expirationInUtc),
    path: '/',
  });

  redirect('/markets');
}

function getGrpcErrorMessage(err: unknown): string {
  if (err && typeof err === 'object' && 'details' in err) {
    const { details } = err as { details: string };
    if (details) return details;
  }
  return 'Login failed. Please try again.';
}

