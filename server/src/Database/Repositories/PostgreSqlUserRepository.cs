using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Database.Entities;
using Metaspesa.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Metaspesa.Database.Repositories;

internal partial class PostgreSqlUserRepository(
  MainContext context,
  ILogger<PostgreSqlUserRepository> logger
) : IUserRepository {
  public async Task<bool> CheckUsernameExistsAsync(
    string username, CancellationToken cancellationToken = default
  ) {
    try {
#pragma warning disable CA1304, CA1862, CA1311
      return await context.Users.AnyAsync(
        u => u.Username.ToUpper() == username.ToUpper(), cancellationToken);
#pragma warning restore CA1304, CA1862, CA1311
    } catch (Exception ex) when (
      ex is NpgsqlException or OperationCanceledException ||
      ex.InnerException is NpgsqlException
    ) {
      LogErrorCheckingUsernameExists(username, ex);
      return true;
    }
  }

  [LoggerMessage(
    LogLevel.Error,
    "Error checking if username '{Username}' exists")]
  partial void LogErrorCheckingUsernameExists(string username, Exception ex);

  public void SaveUser(User user) {
    context.Users.Add(new UserDbEntity {
      Uid = user.Uid,
      Username = user.Username,
      EncryptedPassword = user.HashedPassword,
      RoleId = (int)user.Role,
    });
  }

  public async Task<User?> GetUserByUsernameAsync(
    string username, CancellationToken cancellationToken = default
  ) {
    try {
#pragma warning disable CA1304, CA1862, CA1311
      UserDbEntity? entity = await context.Users
        .Include(u => u.Role)
        .FirstOrDefaultAsync(
          u => u.Username.ToUpper() == username.ToUpper(), cancellationToken);
#pragma warning restore CA1304, CA1862, CA1311

      if (entity is null) {
        return null;
      }

      return Enum.TryParse(entity.Role.Name, out Role role)
        ? new User(entity.Uid, entity.Username, entity.EncryptedPassword, role)
        : null;
    } catch (Exception ex) when (
      ex is NpgsqlException or OperationCanceledException ||
      ex.InnerException is NpgsqlException
    ) {
      LogErrorGettingUserByUsername(username, ex);
      return null;
    }
  }

  [LoggerMessage(
    LogLevel.Error,
    "Error getting user by username '{Username}'")]
  partial void LogErrorGettingUserByUsername(string username, Exception ex);
}
