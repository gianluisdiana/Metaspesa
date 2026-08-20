namespace Metaspesa.Domain.Identity.Errors;

public class PasswordMissingLowercaseException : IdentityDomainException {
  public PasswordMissingLowercaseException()
    : base(
      "User.PasswordMissingLowercase",
      "Password must contain at least one lowercase letter.") { }

  public PasswordMissingLowercaseException(string message) : base(message) { }

  public PasswordMissingLowercaseException(string message, Exception innerException)
    : base(message, innerException) { }
}