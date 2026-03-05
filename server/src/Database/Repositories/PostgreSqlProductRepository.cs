using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Database.Entities;
using Metaspesa.Domain.Shopping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Metaspesa.Database.Repositories;

internal partial class PostgreSqlProductRepository(
  MainContext context,
  ILogger<PostgreSqlProductRepository> logger
) : IProductRepository {
  public async Task<List<RegisteredItem>> GetRegisteredItemsAsync(
    Guid userUid, CancellationToken cancellationToken
  ) {
    try {
      return await context.RegisteredItems
        .Where(ri => ri.UserUid == userUid)
        .Select(ri => new RegisteredItem(
          Name: ri.Name,
          Quantity: ri.Quantity,
          LastPrice: ri.History.OrderByDescending(h => h.CreatedAt).First().Price
        ))
        .ToListAsync(cancellationToken);
    } catch (Exception ex) when (
      ex is NpgsqlException or OperationCanceledException ||
      ex.InnerException is NpgsqlException
    ) {
      LogErrorGettingRegisteredItems(userUid, ex);
      return [];
    }
  }

  [LoggerMessage(
    LogLevel.Error,
    "Couldn't get registered items for user {UserUid}")]
  partial void LogErrorGettingRegisteredItems(Guid userUid, Exception ex);

  public void RegisterItems(
    Guid userUid, IReadOnlyCollection<ShoppingItem> shoppingItems
  ) {
    try {
      IEnumerable<RegisteredItemDbEntity> registeredItems = shoppingItems.Select(i => new RegisteredItemDbEntity {
        UserUid = userUid,
        Name = i.Name,
        Quantity = i.Quantity,
        History = [
          new() {
            Price = i.Price ?? 0,
            CreatedAt = DateTime.UtcNow,
          },
        ],
      });

      context.RegisteredItems.AddRange(registeredItems);
    } catch (Exception ex) when (
      ex is NpgsqlException or OperationCanceledException ||
      ex.InnerException is NpgsqlException
    ) {
      LogErrorRegisteringItems(
        string.Join(", ", shoppingItems.Select(i => i.Name)),
        userUid,
        ex
      );
    }
  }

  [LoggerMessage(
    LogLevel.Error,
    "Couldn't register items {ItemNames} for user {UserUid}")]
  partial void LogErrorRegisteringItems(
    string itemNames, Guid userUid, Exception ex);

  public void UpdateItems(
    Guid userUid, IReadOnlyCollection<ShoppingItem> shoppingItems
  ) {
    try {
      IEnumerable<string> itemNames = shoppingItems
        .Select(i => i.Name.ToUpperInvariant());

#pragma warning disable CA1304, CA1311
      var registeredItems = context.RegisteredItems.Where(ri =>
          ri.UserUid == userUid && itemNames.Contains(ri.Name.ToUpper()))
        .Include(ri => ri.History)
        .ToList();
#pragma warning restore CA1304, CA1311

      foreach (RegisteredItemDbEntity registeredItem in registeredItems) {
        ShoppingItem shoppingItem = shoppingItems.Single(i =>
          i.Name.Equals(registeredItem.Name, StringComparison.OrdinalIgnoreCase));

        registeredItem.Quantity = shoppingItem.Quantity;
        registeredItem.History.Add(new RegisteredItemsHistoryDbEntity {
          Price = shoppingItem.Price,
          CreatedAt = DateTime.UtcNow,
        });
      }
    } catch (Exception ex) when (
      ex is NpgsqlException or OperationCanceledException ||
      ex.InnerException is NpgsqlException
    ) {
      LogErrorUpdatingItems(
        string.Join(", ", shoppingItems.Select(i => i.Name)),
        userUid,
        ex
      );
    }
  }

  [LoggerMessage(
    LogLevel.Error,
    "Couldn't update items {ItemNames} for user {UserUid}")]
  partial void LogErrorUpdatingItems(
    string itemNames, Guid userUid, Exception ex);
}