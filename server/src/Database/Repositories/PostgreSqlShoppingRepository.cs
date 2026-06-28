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
  public async Task<List<AShoppingList>> GetShoppingListSummariesAsync(
    Guid userUid, CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync<List<AShoppingList>>(async () => {
    List<string?> listNames = await context.ShoppingListOwnerships
      .Where(o => o.UserUid == userUid)
      .OrderBy(o => o.ShoppingList.Name == null)
      .ThenBy(o => o.ShoppingList.Name)
      .Select(o => o.ShoppingList.Name)
      .ToListAsync(cancellationToken);

    return [.. listNames.Select(name => new AShoppingList(name, []))];
  }, "Couldn't get shopping list summaries.");

  public async Task<AShoppingList?> GetShoppingListAsync(
    Guid userUid, string? shoppingListName, CancellationToken cancellationToken
  ) => await PostgreSqlExceptionMapper.MapAsync(
    async () => await context.ShoppingListOwnerships
      .Where(sl => sl.UserUid == userUid && (
        sl.ShoppingList.Name == null && shoppingListName == null ||
        sl.ShoppingList.Name != null &&
        shoppingListName != null &&
        EF.Functions.ILike(sl.ShoppingList.Name, shoppingListName)
      ))
      .Select(sl => new AShoppingList(
        Name: sl.ShoppingList.Name,
        Items: sl.ShoppingList.Items
          .Where(i => i.DeletedAt == null)
          .Select(i => new AShoppingItem(
            i.ProductFormatId,
            i.Amount,
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
    var list = new ShoppingListDbEntity {
      Name = name,
      IsTemporary = name is null,
    };
    context.ShoppingLists.Add(list);
    context.ShoppingListOwnerships.Add(new ShoppingListOwnershipDbEntity {
      UserUid = userUid,
      ShoppingList = list,
    });
  }

  public void UpdateShoppingListName(Guid userUid, string? listName, string? newName) =>
    PostgreSqlExceptionMapper.Map(() => {
      ShoppingListDbEntity list = context.ShoppingListOwnerships
        .Where(o => o.UserUid == userUid && (
          o.ShoppingList.Name == null && listName == null ||
          o.ShoppingList.Name != null &&
          listName != null &&
          EF.Functions.ILike(o.ShoppingList.Name, listName)
        ))
        .Select(o => o.ShoppingList)
        .First();

      list.Name = newName;
      list.IsTemporary = newName is null;
    }, "Couldn't update shopping list.");

  public void AddItemsToList(
    Guid userUid, string? listName, IReadOnlyCollection<AShoppingItem> items
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
      ProductFormatId = i.ReferenceUid,
      Amount = i.Amount,
      IsChecked = i.IsChecked,
    }));
  }, "Couldn't add items to shopping list.");

  public async Task<bool> CheckItemExistsAsync(
    Guid userUid,
    string? listName,
    int referenceUid,
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
          i.ProductFormatId == referenceUid,
        cancellationToken),
    "Couldn't check if shopping item exists.");

  public async Task<AShoppingItem?> GetItemAsync(
    Guid userUid,
    string? listName,
    int referenceUid,
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
        i.ProductFormatId == referenceUid)
      .Select(i => new AShoppingItem(
        i.ProductFormatId,
        i.Amount,
        i.IsChecked))
      .FirstOrDefaultAsync(cancellationToken),
    "Couldn't get shopping item.");

  public void UpdateItem(
    Guid userUid, string? listName, AShoppingItem update
  ) => PostgreSqlExceptionMapper.Map(() => {
    ShoppingItemDbEntity item = context.ShoppingItems
      .Where(i => i.DeletedAt == null &&
        i.ShoppingList.Ownerships.Any(o => o.UserUid == userUid) && (
          i.ShoppingList.Name == null && listName == null ||
          i.ShoppingList.Name != null &&
          listName != null &&
          EF.Functions.ILike(i.ShoppingList.Name, listName)
        ) &&
        i.ProductFormatId == update.ReferenceUid)
      .Single();

    if (item.IsChecked != update.IsChecked) {
      item.IsChecked = update.IsChecked;
    }
    if (item.Amount != update.Amount) {
      item.Amount = update.Amount;
    }
  }, "Couldn't update shopping item.");

  public void RemoveItem(Guid userUid, string? listName, int referenceUid) =>
    PostgreSqlExceptionMapper.Map(() => {
      ShoppingItemDbEntity item = context.ShoppingItems
        .Where(i => i.DeletedAt == null &&
          i.ShoppingList.Ownerships.Any(o => o.UserUid == userUid) && (
            i.ShoppingList.Name == null && listName == null ||
            i.ShoppingList.Name != null &&
            listName != null &&
            EF.Functions.ILike(i.ShoppingList.Name, listName)
          ) &&
          i.ProductFormatId == referenceUid)
        .First();

      item.DeletedAt = clock.GetCurrentTime();
    }, "Couldn't remove shopping item.");

  public void RecordShoppingList(Guid userUid, AShoppingList shoppingList) {
    PostgreSqlExceptionMapper.Map(() => {
      List<AShoppingItem> checkedItems = [.. shoppingList.Items.Where(i => i.IsChecked)];

      DateTime now = clock.GetCurrentTime();
      List<PurchaseItemDbEntity> purchaseItems = [];
      foreach (AShoppingItem ci in checkedItems) {
        PurchaseItemDbEntity item = context.PriceSnapshots
          .Where(ps => ps.ProductFormatId == ci.ReferenceUid)
          .OrderByDescending(ps => ps.ObservedAt)
          .ThenByDescending(ps => ps.Id)
          .Select(ps => new PurchaseItemDbEntity {
            Amount = ci.Amount,
            PriceSnapshotId = ps.Id,
          })
          .Single();
        purchaseItems.Add(item);
      }
      context.Purchases.Add(new PurchaseDbEntity {
        UserUid = userUid,
        ShoppingListId = context.ShoppingListOwnerships
          .Where(o => o.UserUid == userUid && (
            o.ShoppingList.Name == null && shoppingList.Name == null ||
            o.ShoppingList.Name != null &&
            shoppingList.Name != null &&
            EF.Functions.ILike(o.ShoppingList.Name, shoppingList.Name)
          ))
          .Select(o => o.ShoppingListId)
          .First(),
        PurchasedAt = now,
        Items = purchaseItems,
      });
    }, "Couldn't record shopping list.");
  }

  public void ResetShoppingList(Guid userUid, string? shoppingListName) {
    // update all checked items to be unchecked
    PostgreSqlExceptionMapper.Map(() => {
      var checkedItems = context.ShoppingItems
        .Where(i => i.DeletedAt == null &&
          i.IsChecked &&
          i.ShoppingList.Ownerships.Any(o => o.UserUid == userUid) && (
            i.ShoppingList.Name == null && shoppingListName == null ||
            i.ShoppingList.Name != null &&
            shoppingListName != null &&
            EF.Functions.ILike(i.ShoppingList.Name, shoppingListName)
          ))
        .ToList();

      foreach (ShoppingItemDbEntity item in checkedItems) {
        item.IsChecked = false;
      }
    }, "Couldn't reset shopping list.");
  }
}
