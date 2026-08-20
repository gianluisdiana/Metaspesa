namespace Metaspesa.Domain.Identity.Errors;

public class PasswordMissingDigitException : IdentityDomainException {
  public PasswordMissingDigitException()
    : base(
      "User.PasswordMissingDigit",
      "Password must contain at least one digit.") { }

  public PasswordMissingDigitException(string message) : base(message) { }

  public PasswordMissingDigitException(string message, Exception innerException)
    : base(message, innerException) { }
}