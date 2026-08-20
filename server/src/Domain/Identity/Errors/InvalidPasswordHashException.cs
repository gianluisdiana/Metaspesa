namespace Metaspesa.Domain.Identity.Errors;

public class InvalidPasswordHashException : IdentityDomainException {
  public InvalidPasswordHashException()
    : base("User.PasswordHashEmpty", "Password hash must not be empty.") { }

  public InvalidPasswordHashException(string message) : base(message) { }

  public InvalidPasswordHashException(string message, Exception innerException)
    : base(message, innerException) { }
}