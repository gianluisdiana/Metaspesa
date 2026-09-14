import { CredentialsMessage } from '@/lib/auth-contracts';
import AuthService from '@/lib/auth-service';

interface ProblemDetails {
  title?: unknown;
}

type Fetcher = (input: string, init: RequestInit) => Promise<Response>;

export default class RestAuthService implements AuthService {
  private readonly baseUrl: string;

  public constructor(
    baseUrl = process.env.NEXT_PUBLIC_REST_API_URL ??
      'http://localhost:4001/api/v1',
    private readonly fetcher: Fetcher = globalThis.fetch.bind(globalThis),
  ) {
    this.baseUrl = baseUrl.replace(/\/$/, '');
  }

  public async login(credentials: CredentialsMessage): Promise<void> {
    await this.post('/auth/sessions', credentials, 'Login failed.');
  }

  public async register(credentials: CredentialsMessage): Promise<void> {
    await this.post('/auth/registrations', credentials, 'Registration failed.');
  }

  private async post(
    path: string,
    credentials: CredentialsMessage,
    fallbackMessage: string,
  ): Promise<void> {
    const response = await this.fetcher(`${this.baseUrl}${path}`, {
      body: JSON.stringify(credentials),
      credentials: 'include',
      headers: { 'Content-Type': 'application/json' },
      method: 'POST',
    });
    if (response.ok) {
      return;
    }

    let message = fallbackMessage;
    try {
      const problem = (await response.json()) as ProblemDetails;
      if (typeof problem.title === 'string' && problem.title.length > 0) {
        message = problem.title;
      }
    } catch {
      // Malformed error response uses stable fallback.
    }
    throw new Error(message);
  }
}
