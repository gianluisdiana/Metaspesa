using System.Diagnostics;
using Metaspesa.Application.Abstractions.Core;
using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Database.Entities;
using Metaspesa.Domain.Shopping;
using Microsoft.EntityFrameworkCore;

namespace Metaspesa.Database.Repositories;

internal partial class PostgreSqlShoppingRepository(
  MainContext context,
  IClock clock
) : IShoppingRepository {
  public async Task<List<ShoppingList>> GetShoppingListSummariesAsync(
    Guid userUid, CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync<List<ShoppingList>>(async () => {
    List<string?> listNames = await context.ShoppingListOwnerships
      .Where(o => o.UserUid == userUid)
      .OrderBy(o => o.ShoppingList.Name == null)
      .ThenBy(o => o.ShoppingList.Name)
      .Select(o => o.ShoppingList.Name)
      .ToListAsync(cancellationToken);

    return [.. listNames.Select(name => new ShoppingList(name, []))];
  }, "Couldn't get shopping list summaries.");

  public async Task<ShoppingList?> GetShoppingListAsync(
    Guid userUid, string? shoppingListName, CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(
    async () => await context.ShoppingListOwnerships
      .Where(sl => sl.UserUid == userUid && (
        sl.ShoppingList.Name == null && shoppingListName == null ||
        sl.ShoppingList.Name != null &&
        shoppingListName != null &&
        EF.Functions.ILike(sl.ShoppingList.Name, shoppingListName)
      ))
      .Select(sl => new ShoppingList(
        Name: sl.ShoppingList.Name,
        Items: sl.ShoppingList.Items
          .Where(i => i.DeletedAt == null)
          .Select(i => new ShoppingItem(
            Name: i.Name,
            Quantity: Quantity.FromNullable(i.Quantity),
            Price: new Price(i.Price),
            IsChecked: i.IsChecked
          )).ToList()
      ))
      .FirstOrDefaultAsync(cancellationToken),
    "Couldn't get shopping list.");

  public async Task<bool> CheckShoppingListExistAsync(
    Guid userUid, string? shoppingListName, CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(
    async () => await context.ShoppingListOwnerships
      .AnyAsync(
        o => o.UserUid == userUid && (
          o.ShoppingList.Name == null && shoppingListName == null ||
          o.ShoppingList.Name != null &&
          shoppingListName != null &&
          EF.Functions.ILike(o.ShoppingList.Name, shoppingListName)
        ),
        cancellationToken),
    "Couldn't check if shopping list exists.");

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
  ) => PostgreSqlExceptionMapper.Map(() => {
    ShoppingListDbEntity list = context.ShoppingListOwnerships
      .Where(o => o.UserUid == userUid && (
        o.ShoppingList.Name == null && listName == null ||
        o.ShoppingList.Name != null &&
        listName != null &&
        EF.Functions.ILike(o.ShoppingList.Name, listName)
      ))
      .Select(o => o.ShoppingList)
      .First();

    context.ShoppingItems.AddRange(items.Select(i => new ShoppingItemDbEntity {
      ShoppingListId = list.Id,
      Name = i.Name,
      Quantity = i.Quantity?.Value,
      Price = i.Price.Value,
      IsChecked = i.IsChecked,
    }));
  }, "Couldn't add items to shopping list.");

  public async Task<bool> CheckItemExistsAsync(
    Guid userUid,
    string? listName,
    string itemName,
    CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(
    async () => await context.ShoppingItems
      .AnyAsync(
        i => i.DeletedAt == null &&
          i.ShoppingList.Ownerships.Any(o => o.UserUid == userUid) && (
            i.ShoppingList.Name == null && listName == null ||
            i.ShoppingList.Name != null &&
            listName != null &&
            EF.Functions.ILike(i.ShoppingList.Name, listName)
          ) &&
          EF.Functions.ILike(i.Name, itemName),
        cancellationToken),
    "Couldn't check if shopping item exists.");

  public async Task<ShoppingItem?> GetItemAsync(
    Guid userUid,
    string? listName,
    string itemName,
    CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(
    async () => await context.ShoppingItems
      .Where(i => i.DeletedAt == null &&
        i.ShoppingList.Ownerships.Any(o => o.UserUid == userUid) && (
          i.ShoppingList.Name == null && listName == null ||
          i.ShoppingList.Name != null &&
          listName != null &&
          EF.Functions.ILike(i.ShoppingList.Name, listName)
        ) &&
        EF.Functions.ILike(i.Name, itemName))
      .Select(i => new ShoppingItem(
        i.Name,
        Quantity.FromNullable(i.Quantity),
        new Price(i.Price),
        i.IsChecked))
      .FirstOrDefaultAsync(cancellationToken),
    "Couldn't get shopping item.");

  public void UpdateItem(
    Guid userUid, string? listName, string originalItemName, ShoppingItem update
  ) => PostgreSqlExceptionMapper.Map(() => {
    ShoppingItemDbEntity item = context.ShoppingItems
      .Where(i => i.DeletedAt == null &&
        i.ShoppingList.Ownerships.Any(o => o.UserUid == userUid) && (
          i.ShoppingList.Name == null && listName == null ||
          i.ShoppingList.Name != null &&
          listName != null &&
          EF.Functions.ILike(i.ShoppingList.Name, listName)
        ) &&
        EF.Functions.ILike(i.Name, originalItemName))
      .First();

    item.Name = update.Name;
    item.Quantity = update.Quantity?.Value;
    item.Price = update.Price.Value;
    item.IsChecked = update.IsChecked;
  }, "Couldn't update shopping item.");

  public void RemoveItem(Guid userUid, string? listName, string itemName) =>
    PostgreSqlExceptionMapper.Map(() => {
    ShoppingItemDbEntity item = context.ShoppingItems
      .Where(i => i.DeletedAt == null &&
        i.ShoppingList.Ownerships.Any(o => o.UserUid == userUid) && (
          i.ShoppingList.Name == null && listName == null ||
          i.ShoppingList.Name != null &&
          listName != null &&
          EF.Functions.ILike(i.ShoppingList.Name, listName)
        ) &&
        EF.Functions.ILike(i.Name, itemName))
      .First();

    item.DeletedAt = clock.GetCurrentTime();
  }, "Couldn't remove shopping item.");

  public void RecordShoppingList(Guid userUid, ShoppingList shoppingList) {
    Debug.Assert(shoppingList.HasCheckedItems());

    PostgreSqlExceptionMapper.Map(() => {
    List<ShoppingItem> checkedItems = [.. shoppingList.Items.Where(i => i.IsChecked)];

    DateTime now = clock.GetCurrentTime();
    context.Purchases.AddRange(checkedItems.Select(ci => new PurchaseDbEntity {
      UserUid = userUid,
      RegisteredItemId = 0, // Temporary
      PricePaid = ci.Price.Value,
      Quantity = ci.Quantity?.Value,
      PurchasedAt = now,
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
      itemEntity.DeletedAt = now;
    }
    }, "Couldn't record shopping list.");
  }
}
