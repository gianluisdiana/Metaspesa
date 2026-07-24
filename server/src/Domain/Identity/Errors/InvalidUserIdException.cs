namespace Metaspesa.Domain.Identity.Errors;

public class InvalidUserIdException : IdentityDomainException {
  public InvalidUserIdException()
    : base("User.IdEmpty", "User id must not be empty.") { }

  public InvalidUserIdException(string message) : base(message) { }

  public InvalidUserIdException(string message, Exception innerException)
    : base(message, innerException) { }
}
