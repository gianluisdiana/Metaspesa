import { CredentialsMessage, LoginResultMessage } from './auth-contracts';

export default interface AuthService {
  login(credentials: CredentialsMessage): Promise<LoginResultMessage>;
  register(credentials: CredentialsMessage): Promise<void>;
}
