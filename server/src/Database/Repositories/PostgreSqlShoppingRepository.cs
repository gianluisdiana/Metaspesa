using System.Diagnostics;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Database.Entities;
using Metaspesa.Domain.Shopping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Metaspesa.Database.Repositories;

internal partial class PostgreSqlShoppingRepository(
  MainContext context,
  ILogger<PostgreSqlShoppingRepository> logger
) : IShoppingRepository {
  public async Task<ShoppingList?> GetCurrentShoppingListAsync(
    Guid userUid, CancellationToken cancellationToken
  ) {
    try {
      return await context.ShoppingListOwnerships
        .Where(sl => sl.UserUid == userUid)
        .OrderByDescending(sl => sl.LastTimeUsed)
        .Select(sl => new ShoppingList(
          Name: sl.ShoppingList.Name,
          Items: sl.ShoppingList.Items.Select(i => new ShoppingItem(
            Name: i.Name,
            Quantity: i.Quantity,
            Price: i.Price,
            IsChecked: i.IsChecked
          )).ToList()
        ))
        .FirstOrDefaultAsync(cancellationToken);
    } catch (Exception ex) when (
        ex is NpgsqlException or OperationCanceledException ||
        ex.InnerException is NpgsqlException
      ) {
      LogErrorGettingCurrentShoppingList(userUid, ex);
      return null;
    }
  }

  [LoggerMessage(
    LogLevel.Error,
    "Couldn't get current shopping list for user {UserUid}")]
  partial void LogErrorGettingCurrentShoppingList(Guid userUid, Exception ex);

  public void RecordShoppingList(Guid userUid, ShoppingList shoppingList) {
    Debug.Assert(shoppingList.IsNamed);

    try {
      ShoppingListDbEntity? shoppingListEntity = context.ShoppingLists
        .Where(sl => sl.Ownerships.Any(o => o.UserUid == userUid) &&
          sl.Name == shoppingList.Name)
        .Include(sl => sl.Items)
        .FirstOrDefault();

      if (shoppingListEntity is not null) {
        shoppingListEntity.Items.Clear();
        shoppingListEntity.Items = [.. shoppingList.Items.Select(i => new ShoppingItemDbEntity {
          Name = i.Name,
          Quantity = i.Quantity,
          Price = i.Price,
          IsChecked = i.IsChecked,
        })];

        return;
      }

      context.ShoppingLists.Add(new ShoppingListDbEntity {
        Ownerships = [new ShoppingListOwnershipDbEntity {
          UserUid = userUid,
          LastTimeUsed = DateTime.UtcNow,
        }],
        Name = shoppingList.Name,
        Items = [.. shoppingList.Items.Select(i => new ShoppingItemDbEntity {
          Name = i.Name,
          Quantity = i.Quantity,
          Price = i.Price,
          IsChecked = i.IsChecked,
        })],
      });
    } catch (Exception ex) when (
        ex is NpgsqlException or OperationCanceledException ||
        ex.InnerException is NpgsqlException
      ) {
      LogErrorRecordingShoppingList(userUid, shoppingList.Name, ex);
    }
  }

  [LoggerMessage(
    LogLevel.Error,
    "Couldn't record shopping list '{ShoppingListName}' for user {UserUid}")]
  partial void LogErrorRecordingShoppingList(
    Guid userUid, string shoppingListName, Exception ex);
}
