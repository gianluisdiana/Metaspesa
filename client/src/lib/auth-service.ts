import { CredentialsMessage } from './auth-contracts';

export default interface AuthService {
  login(credentials: CredentialsMessage): Promise<void>;
  register(credentials: CredentialsMessage): Promise<void>;
}
