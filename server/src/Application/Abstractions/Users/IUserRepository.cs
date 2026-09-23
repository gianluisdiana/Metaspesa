using Metaspesa.Domain.Identity;

namespace Metaspesa.Application.Abstractions.Users;

public interface IUserRepository {
  Task<bool> CheckUsernameExistsAsync(
    string username, CancellationToken cancellationToken = default);
  Task SaveAsync(User user, CancellationToken cancellationToken = default);
  Task<User?> GetUserByUsernameAsync(
    Username username, CancellationToken cancellationToken = default);
}