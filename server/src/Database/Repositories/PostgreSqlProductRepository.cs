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
  public async Task<List<Product>> GetRegisteredItemsAsync(
    Guid userUid, CancellationToken cancellationToken
  ) {
    try {
      return await context.RegisteredItems
        .Where(ri => ri.UserUid == userUid)
        .Select(ri => new Product(
          Name: ri.Name,
          Quantity: ri.Quantity,
          Price: new Price(ri.LastKnownPrice)
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
      IEnumerable<RegisteredItemDbEntity> registeredItems = shoppingItems
        .Select(i => new RegisteredItemDbEntity {
          UserUid = userUid,
          Name = i.Name,
          Quantity = i.Quantity,
          LastKnownPrice = i.Price.Value,
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

  public void UpdateRegisteredItems(
    Guid userUid, IReadOnlyCollection<ShoppingItem> shoppingItems
  ) {
    try {
      IEnumerable<string> itemNames = shoppingItems
        .Select(i => i.Name.ToUpperInvariant());

#pragma warning disable CA1304, CA1311
      var registeredItems = context.RegisteredItems.Where(ri =>
          ri.UserUid == userUid && itemNames.Contains(ri.Name.ToUpper()))
        .ToList();
#pragma warning restore CA1304, CA1311

      foreach (RegisteredItemDbEntity registeredItem in registeredItems) {
        ShoppingItem shoppingItem = shoppingItems.Single(i =>
          i.Name.Equals(registeredItem.Name, StringComparison.OrdinalIgnoreCase));

        registeredItem.Quantity = shoppingItem.Quantity;
        registeredItem.LastKnownPrice = !shoppingItem.Price.IsZero()
          ? shoppingItem.Price.Value
          : registeredItem.LastKnownPrice;
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

  [LoggerMessage(
    LogLevel.Error,
    "Couldn't delete items {ItemNames} for user {UserUid}")]
  partial void LogErrorDeletingItems(
    Guid userUid, string itemNames, Exception ex);

}
