namespace Metaspesa.Domain.Identity.Errors;

public class UsernameAlreadyExistsException : IdentityDomainException {
  public UsernameAlreadyExistsException()
    : base("User.UsernameAlreadyExists", "Username is already taken.") { }

  public UsernameAlreadyExistsException(string username)
    : base("User.UsernameAlreadyExists", $"Username '{username}' is already taken.") {
    Username = username;
  }

  public UsernameAlreadyExistsException(string message, Exception innerException)
    : base(message, innerException) { }

  public string Username { get; } = string.Empty;
}
