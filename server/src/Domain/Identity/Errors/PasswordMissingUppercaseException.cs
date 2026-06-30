namespace Metaspesa.Domain.Identity.Errors;

public class PasswordMissingUppercaseException : IdentityDomainException {
  public PasswordMissingUppercaseException()
    : base(
      "User.PasswordMissingUppercase",
      "Password must contain at least one uppercase letter.") { }

  public PasswordMissingUppercaseException(string message) : base(message) { }

  public PasswordMissingUppercaseException(string message, Exception innerException)
    : base(message, innerException) { }
}
