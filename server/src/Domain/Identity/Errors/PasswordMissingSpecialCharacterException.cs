namespace Metaspesa.Domain.Identity.Errors;

public class PasswordMissingSpecialCharacterException : IdentityDomainException {
  public PasswordMissingSpecialCharacterException()
    : base(
      "User.PasswordMissingSpecialChar",
      "Password must contain at least one special character.") { }

  public PasswordMissingSpecialCharacterException(string message) : base(message) { }

  public PasswordMissingSpecialCharacterException(string message, Exception innerException)
    : base(message, innerException) { }
}
