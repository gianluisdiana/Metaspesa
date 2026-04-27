namespace Metaspesa.Domain.Users;

public record User(
  Guid Uid,
  string Username,
  string HashedPassword,
  Role Role);
