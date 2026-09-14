import RestAuthService from '@/infrastructure/rest-auth-service';
import { Credentials } from '@/lib/auth-domain';
import { getAuthErrorMessage } from '@/lib/auth-errors';

export type RegisterState = { error: string } | { registered: true } | null;

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

  const service = new RestAuthService();
  try {
    await service.register({ password, username });
  } catch (err) {
    return {
      error: getAuthErrorMessage(err, 'Registration failed. Please try again.'),
    };
  }

  return { registered: true };
}
