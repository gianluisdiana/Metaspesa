using Metaspesa.Domain.Identity;

namespace Metaspesa.Application.Abstractions.Users;

public interface IUserRepository {
  Task<bool> CheckUsernameExistsAsync(
    Username username, CancellationToken cancellationToken = default);
  void SaveUser(User user);
  Task<User?> GetUserByUsernameAsync(
    Username username, CancellationToken cancellationToken = default);
}