'use server';

import { redirect } from 'next/navigation';

import GrpcAuthService from '@/infrastructure/grpc-auth-service';
import { Credentials } from '@/lib/auth-domain';

export type RegisterState = { error: string } | null;

export async function registerAction(
  _: RegisterState,
  formData: FormData,
): Promise<RegisterState> {
  const username = (formData.get('username') as string) ?? '';
  const password = (formData.get('password') as string) ?? '';
  const confirmPassword = (formData.get('confirm-password') as string) ?? '';

  const credentials = new Credentials(username, password);
  if (!credentials.hasValidUsername()) {
    return { error: 'Username cannot be empty.' };
  }
  if (!credentials.hasValidPassword()) {
    return {
      error:
        'Password must be at least 10 characters and include uppercase, lowercase, digit, and special character.',
    };
  }
  if (password !== confirmPassword) {
    return { error: 'Passwords do not match.' };
  }

  const service = new GrpcAuthService();
  try {
    await service.register({ password, username });
  } catch (err) {
    return { error: getGrpcErrorMessage(err) };
  }

  redirect('/auth/login');
}

function getGrpcErrorMessage(err: unknown): string {
  if (err && typeof err === 'object' && 'details' in err) {
    const { details } = err as { details: string };
    if (details) return details;
  }
  return 'Registration failed. Please try again.';
}
