using Metaspesa.Database.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Metaspesa.Database.Repositories;

internal static class PostgreSqlExceptionMapper {
  public static async Task<T> MapAsync<T>(
    Func<Task<T>> action,
    string message
  ) {
    try {
      return await action();
    } catch (Exception ex) when (IsDatabaseFailure(ex)) {
      throw new DatabaseException(message, ex);
    }
  }

  public static async Task MapAsync(
    Func<Task> action,
    string message
  ) {
    try {
      await action();
    } catch (Exception ex) when (IsDatabaseFailure(ex)) {
      throw new DatabaseException(message, ex);
    }
  }

  public static void Map(
    Action action,
    string message
  ) {
    try {
      action();
    } catch (Exception ex) when (IsDatabaseFailure(ex)) {
      throw new DatabaseException(message, ex);
    }
  }

  private static bool IsDatabaseFailure(Exception exception) =>
    exception is NpgsqlException or DbUpdateException or TimeoutException ||
    exception.InnerException is NpgsqlException;
}
