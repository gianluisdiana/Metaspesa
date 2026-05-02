const MIN_PASSWORD_LENGTH = 10;

export class Credentials {
  public constructor(
    public username: string,
    public password: string,
  ) {}

  public hasValidUsername(): boolean {
    return this.username.trim().length > 0;
  }

  public passwordHasMinimumLength(): boolean {
    return this.password.length >= MIN_PASSWORD_LENGTH;
  }

  public passwordHasUppercase(): boolean {
    return /[A-Z]/.test(this.password);
  }

  public passwordHasLowercase(): boolean {
    return /[a-z]/.test(this.password);
  }

  public passwordHasDigit(): boolean {
    return /\d/.test(this.password);
  }

  public passwordHasSpecialCharacter(): boolean {
    return /[^a-zA-Z0-9]/.test(this.password);
  }

  public hasValidPassword(): boolean {
    return (
      this.passwordHasMinimumLength() &&
      this.passwordHasUppercase() &&
      this.passwordHasLowercase() &&
      this.passwordHasDigit() &&
      this.passwordHasSpecialCharacter()
    );
  }

  public isValid(): boolean {
    return this.hasValidUsername() && this.hasValidPassword();
  }
}

export class LoginResult {
  public constructor(
    public token: string,
    public expirationInUtc: Date,
  ) {}

  public isExpired(): boolean {
    return new Date() > this.expirationInUtc;
  }
}

