export interface CredentialsMessage {
  username: string;
  password: string;
}

export interface LoginResultMessage {
  token: string;
  expirationInUtc: string;
}
