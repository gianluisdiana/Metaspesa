import RestAuthService from '@/infrastructure/rest-auth-service';
import { Credentials } from '@/lib/auth-domain';
import { getAuthErrorMessage } from '@/lib/auth-errors';

export type LoginState = { authenticated: true } | { error: string } | null;

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
  const service = new RestAuthService();
  try {
    await service.login({ password, username });
  } catch (err) {
    return {
      error: getAuthErrorMessage(err, 'Login failed. Please try again.'),
    };
  }

  return { authenticated: true };
}
