using Metaspesa.Domain.Identity.Errors;

namespace Metaspesa.Domain.Identity;

public class User {
  public UserId Id { get; }
  public Username Username { get; }
  public PasswordHash PasswordHash { get; private set; }
  public Role Role { get; private set; }

  private User(
    UserId id, Username username, PasswordHash passwordHash, Role role
  ) {
    EnsureValidRole(role);

    Id = id;
    Username = username;
    PasswordHash = passwordHash;
    Role = role;
  }

  public static User CreateShopper(
    UserId id,
    Username username,
    PasswordHash passwordHash
  ) => new(id, username, passwordHash, Role.Shopper);

  public static User Rehydrate(
    UserId id,
    Username username,
    PasswordHash passwordHash,
    Role role
  ) => new(id, username, passwordHash, role);

  private static void EnsureValidRole(Role role) {
    if (role is Role.None || !Enum.IsDefined(role)) {
      throw new InvalidRoleException(role);
    }
  }
}
