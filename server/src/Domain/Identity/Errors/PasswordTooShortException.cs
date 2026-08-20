namespace Metaspesa.Domain.Identity.Errors;

public class PasswordTooShortException : IdentityDomainException {
  public PasswordTooShortException()
    : base(
      "User.PasswordTooShort",
      $"Password must be at least {PasswordPolicy.MinimumLength} characters.") { }

  public PasswordTooShortException(string message) : base(message) { }

  public PasswordTooShortException(string message, Exception innerException)
    : base(message, innerException) { }
}