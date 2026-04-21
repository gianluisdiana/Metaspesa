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
        .Select(sl => new ShoppingList(
          Name: sl.ShoppingList.Name,
          Items: sl.ShoppingList.Items
            .Where(i => i.DeletedAt == null)
            .Select(i => new ShoppingItem(
              Name: i.Name,
              Quantity: i.Quantity,
              Price: new Price(i.Price),
              IsChecked: false
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

  public async Task<bool> CheckShoppingListExistAsync(
    Guid userUid, string? shoppingListName, CancellationToken cancellationToken
  ) {
    try {
#pragma warning disable CA1862, CA1304, CA1311
      return await context.ShoppingListOwnerships
        .AnyAsync(
          o => o.UserUid == userUid && (
            o.ShoppingList.Name == null && shoppingListName == null ||
            o.ShoppingList.Name != null &&
            shoppingListName != null &&
            o.ShoppingList.Name.ToUpper() == shoppingListName.ToUpper()
          ),
          cancellationToken);
#pragma warning restore CA1862, CA1304, CA1311
    } catch (Exception ex) when (
        ex is NpgsqlException or OperationCanceledException ||
        ex.InnerException is NpgsqlException
      ) {
      LogErrorCheckingShoppingListExists(userUid, shoppingListName, ex);
      return false;
    }
  }

  [LoggerMessage(
    LogLevel.Error,
    "Couldn't check if shopping list '{ShoppingListName}' exists for user {UserUid}")]
  partial void LogErrorCheckingShoppingListExists(
    Guid userUid, string? shoppingListName, Exception ex);

  public void CreateShoppingList(Guid userUid, string? name) {
    var list = new ShoppingListDbEntity { Name = name };
    context.ShoppingLists.Add(list);
    context.ShoppingListOwnerships.Add(new ShoppingListOwnershipDbEntity {
      UserUid = userUid,
      ShoppingList = list,
    });
  }

  public void AddItemsToList(
    Guid userUid, string? listName, IReadOnlyCollection<ShoppingItem> items
  ) {
#pragma warning disable CA1862, CA1304, CA1311
    ShoppingListDbEntity list = context.ShoppingListOwnerships
      .Where(o => o.UserUid == userUid && (
        o.ShoppingList.Name == null && listName == null ||
        o.ShoppingList.Name != null &&
        listName != null &&
        o.ShoppingList.Name.ToUpper() == listName.ToUpper()
      ))
      .Select(o => o.ShoppingList)
      .First();
#pragma warning restore CA1862, CA1304, CA1311

    context.ShoppingItems.AddRange(items.Select(i => new ShoppingItemDbEntity {
      ShoppingListId = list.Id,
      Name = i.Name,
      Quantity = i.Quantity,
      Price = i.Price.Value,
      IsChecked = i.IsChecked,
    }));
  }

  public void RecordShoppingList(Guid userUid, ShoppingList shoppingList) {
    Debug.Assert(shoppingList.HasCheckedItems());

    try {
      List<ShoppingItem> checkedItems = [.. shoppingList.Items.Where(i => i.IsChecked)];

      context.Purchases.AddRange(checkedItems.Select(ci => new PurchaseDbEntity {
        UserUid = userUid,
        RegisteredItemId = 0, // Temporary
        PricePaid = ci.Price.Value,
        Quantity = ci.Quantity,
        PurchasedAt = DateTime.UtcNow,
      }));

#pragma warning disable CA1304, CA1311
      var purchasedEntities = context.ShoppingItems
        .Where(i => i.ShoppingList.Ownerships.Any(o => o.UserUid == userUid) &&
          i.ShoppingList.Name == shoppingList.Name &&
          checkedItems.Select(ci => ci.Name.ToUpper()).Contains(i.Name.ToUpper()))
        .Include(i => i.ShoppingList)
        .ToList();
#pragma warning restore CA1304, CA1311

      foreach (ShoppingItemDbEntity itemEntity in purchasedEntities) {
        itemEntity.DeletedAt = DateTime.UtcNow;
      }
    } catch (Exception ex) when (
        ex is NpgsqlException or OperationCanceledException ||
        ex.InnerException is NpgsqlException
      ) {
      LogErrorRecordingShoppingList(userUid, shoppingList.Name, ex);
    }
  }

  [LoggerMessage(
    LogLevel.Error,
    "Couldn't record shopping list '{ShoppingListName}' of user {UserUid}")]
  partial void LogErrorRecordingShoppingList(
    Guid userUid, string? shoppingListName, Exception ex);
}
