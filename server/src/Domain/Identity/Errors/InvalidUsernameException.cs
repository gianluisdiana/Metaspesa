namespace Metaspesa.Domain.Identity.Errors;

public class InvalidUsernameException : IdentityDomainException {
  public InvalidUsernameException()
    : base("User.UsernameEmpty", "Username must not be empty.") { }

  public InvalidUsernameException(string? username)
    : base("User.UsernameEmpty", "Username must not be empty.") {
    Username = username;
  }

  public InvalidUsernameException(string message, Exception innerException)
    : base(message, innerException) { }

  public string? Username { get; }
}
