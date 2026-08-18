using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Database.Entities;
using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.Shopping;
using Microsoft.EntityFrameworkCore;

namespace Metaspesa.Database.Repositories;

internal class PostgreSqlShoppingListRepository(
  MainContext context,
  IClock clock
) : IShoppingListRepository, IShoppingPurchaseRepository {
  public async Task<IReadOnlyCollection<ShoppingList>> GetByOwnerAsync(
    UserId ownerId, CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync<IReadOnlyCollection<ShoppingList>>(
    async () => {
      List<ShoppingListDbEntity> entities = await context.ShoppingLists
        .AsNoTracking()
        .Include(list => list.Ownerships)
        .Include(list => list.Items)
        .Where(list => list.Ownerships.Any(
          ownership => ownership.UserUid == ownerId.Value))
        .OrderBy(list => list.Name == null)
        .ThenBy(list => list.Name)
        .ToListAsync(cancellationToken);

      return [.. entities.Select(ToDomain)];
    },
    "Couldn't get shopping lists.");

  public async Task<ShoppingList?> GetAsync(
    UserId ownerId,
    ShoppingListName? name,
    CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(
    async () => {
      string? nameValue = name?.Value;
      ShoppingListDbEntity? entity = await context.ShoppingLists
        .AsNoTracking()
        .Include(list => list.Ownerships)
        .Include(list => list.Items)
        .Where(list => list.Ownerships.Any(
          ownership => ownership.UserUid == ownerId.Value) && (
          list.Name == null && nameValue == null ||
          list.Name != null &&
          nameValue != null &&
          EF.Functions.ILike(list.Name, nameValue)
        ))
        .FirstOrDefaultAsync(cancellationToken);

      return entity is null ? null : ToDomain(entity);
    },
    "Couldn't get shopping list.");

  public async Task<bool> ExistsAsync(
    UserId ownerId,
    ShoppingListName? name,
    CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(
    async () => {
      string? nameValue = name?.Value;
      return await context.ShoppingListOwnerships.AnyAsync(
        ownership => ownership.UserUid == ownerId.Value && (
          ownership.ShoppingList.Name == null && nameValue == null ||
          ownership.ShoppingList.Name != null &&
          nameValue != null &&
          EF.Functions.ILike(ownership.ShoppingList.Name, nameValue)
        ),
        cancellationToken);
    },
    "Couldn't check if shopping list exists.");

  public void Add(ShoppingList shoppingList) =>
    PostgreSqlExceptionMapper.Map(() => {
      var entity = new ShoppingListDbEntity {
        Name = shoppingList.Name?.Value,
        IsTemporary = shoppingList.IsTemporary,
        DeletedAt = shoppingList.DeletedAt,
        Ownerships = [.. shoppingList.OwnerIds.Select(ownerId =>
          new ShoppingListOwnershipDbEntity { UserUid = ownerId.Value })],
        Items = [.. shoppingList.Items.Select(ToEntity)],
      };
      context.ShoppingLists.Add(entity);
    }, "Couldn't add shopping list.");

  public async Task UpdateAsync(
    ShoppingList shoppingList, CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(async () => {
    ShoppingListId id = shoppingList.Id ??
      throw new InvalidOperationException("Cannot update an unpersisted shopping list.");
    ShoppingListDbEntity entity = await context.ShoppingLists
      .Include(list => list.Items)
      .SingleAsync(list => list.Id == id.Value, cancellationToken);

    entity.Name = shoppingList.Name?.Value;
    entity.IsTemporary = shoppingList.IsTemporary;
    entity.DeletedAt = shoppingList.DeletedAt;

    var desiredItems = shoppingList.Items.ToDictionary(
      item => item.ProductFormatId.Value);
    foreach (ShoppingItemDbEntity existing in entity.Items.Where(
      item => item.DeletedAt == null)) {
      if (!desiredItems.Remove(existing.ProductFormatId, out ShoppingItem? desired)) {
        existing.DeletedAt = clock.GetCurrentTime();
        continue;
      }

      existing.Amount = desired.Amount.Value;
      existing.IsChecked = desired.IsChecked;
    }

    foreach (ShoppingItem addition in desiredItems.Values) {
      entity.Items.Add(ToEntity(addition));
    }
  }, "Couldn't update shopping list.");

  public void Record(UserId ownerId, ShoppingList shoppingList) =>
    PostgreSqlExceptionMapper.Map(() => {
      var purchaseItems = shoppingList.CheckedItems().Select(checkedItem =>
        context.PriceSnapshots
          .Where(snapshot =>
            snapshot.ProductFormatId == checkedItem.ProductFormatId.Value)
          .OrderByDescending(snapshot => snapshot.ObservedAt)
          .ThenByDescending(snapshot => snapshot.Id)
          .Select(snapshot => new PurchaseItemDbEntity {
            Amount = checkedItem.Amount.Value,
            PriceSnapshotId = snapshot.Id,
          })
          .First()).ToList();

      context.Purchases.Add(new PurchaseDbEntity {
        UserUid = ownerId.Value,
        ShoppingListId = shoppingList.Id?.Value ??
          throw new InvalidOperationException(
            "Cannot record an unpersisted shopping list."),
        PurchasedAt = clock.GetCurrentTime(),
        Items = purchaseItems,
      });
    }, "Couldn't record shopping list.");

  public void Reset(UserId ownerId, ShoppingListName? name) =>
    PostgreSqlExceptionMapper.Map(() => {
      string? nameValue = name?.Value;
      var checkedItems = context.ShoppingItems
        .Where(item => item.DeletedAt == null &&
          item.IsChecked &&
          item.ShoppingList.Ownerships.Any(
            ownership => ownership.UserUid == ownerId.Value) && (
            item.ShoppingList.Name == null && nameValue == null ||
            item.ShoppingList.Name != null &&
            nameValue != null &&
            EF.Functions.ILike(item.ShoppingList.Name, nameValue)
          ))
        .ToList();

      foreach (ShoppingItemDbEntity item in checkedItems) {
        item.IsChecked = false;
      }
    }, "Couldn't reset shopping list.");

  private static ShoppingList ToDomain(ShoppingListDbEntity entity) =>
    ShoppingList.Rehydrate(
      new ShoppingListId(entity.Id),
      entity.Ownerships.Select(ownership => new UserId(ownership.UserUid)),
      entity.Name is null ? null : new ShoppingListName(entity.Name),
      entity.DeletedAt,
      entity.Items
        .Where(item => item.DeletedAt == null)
        .Select(item => new ShoppingItem(
          new ProductFormatId(item.ProductFormatId),
          new PositiveAmount(item.Amount),
          item.IsChecked)));

  private static ShoppingItemDbEntity ToEntity(ShoppingItem item) => new() {
    ProductFormatId = item.ProductFormatId.Value,
    Amount = item.Amount.Value,
    IsChecked = item.IsChecked,
  };
}