using Metaspesa.Application.Abstractions.Users;
using Metaspesa.Database.Entities;
using Metaspesa.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Metaspesa.Database.Repositories;

internal partial class PostgreSqlUserRepository(
  MainContext context
) : IUserRepository {
  public async Task<bool> CheckUsernameExistsAsync(
    string username, CancellationToken cancellationToken = default
  ) => await PostgreSqlExceptionMapper.MapAsync(
    async () => await context.Users.AnyAsync(
      u => EF.Functions.ILike(u.Username, username), cancellationToken),
    "Couldn't check if username exists.");

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
  ) => await PostgreSqlExceptionMapper.MapAsync(async () => {
    UserDbEntity? entity = await context.Users
      .Include(u => u.Role)
      .FirstOrDefaultAsync(
        u => EF.Functions.ILike(u.Username, username), cancellationToken);

    if (entity is null) {
      return null;
    }

    return Enum.TryParse(entity.Role.Name, out Role role)
      ? new User(entity.Uid, entity.Username, entity.EncryptedPassword, role)
      : null;
  }, "Couldn't get user by username.");
}