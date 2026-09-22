using Metaspesa.Domain.Identity;

namespace Metaspesa.Application.Abstractions.Users;

public interface IHasher {
  string Hash(string value);
  PasswordHash HashPassword(string password);
}