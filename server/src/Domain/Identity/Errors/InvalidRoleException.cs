namespace Metaspesa.Domain.Identity.Errors;

public class InvalidRoleException : IdentityDomainException {
  public InvalidRoleException()
    : base("User.RoleInvalid", "Role is invalid.") { }

  public InvalidRoleException(string message) : base(message) { }

  public InvalidRoleException(string message, Exception innerException)
    : base(message, innerException) { }

  public InvalidRoleException(Role role)
    : base("User.RoleInvalid", $"Role '{role}' is invalid.") {
    Role = role;
  }

  public Role Role { get; }
}
