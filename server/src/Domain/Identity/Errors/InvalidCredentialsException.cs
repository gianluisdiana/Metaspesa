namespace Metaspesa.Domain.Identity.Errors;

public class InvalidCredentialsException : IdentityDomainException {
  public InvalidCredentialsException()
    : base("User.InvalidCredentials", "Invalid username or password.") { }

  public InvalidCredentialsException(string message) : base(message) { }

  public InvalidCredentialsException(string message, Exception innerException)
    : base(message, innerException) { }
}
