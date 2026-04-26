using Metaspesa.Domain.Users;

namespace Metaspesa.Application.Abstractions.Users;

public interface IUserRepository {
  Task<bool> CheckUsernameExistsAsync(
    string username, CancellationToken cancellationToken = default);
  void SaveUser(User user);
  Task<User?> GetUserByUsernameAsync(
    string username, CancellationToken cancellationToken = default);
}
