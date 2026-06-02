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
            Name: i.Product.Name,
            Quantity: new Quantity($"{i.ProductHistory.ProductFormat.Quantity}"),
            Price: new Price(i.ProductHistory.Price),
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

    var productHistoryLookup = context.ProductsHistory
      .Where(ph => items.Select(i => i.ReferenceUid).Contains(ph.Id))
      .ToDictionary(ph => ph.Id, ph => ph.ProductId);

    context.ShoppingItems.AddRange(items.Select(i => new ShoppingItemDbEntity {
      ShoppingListId = list.Id,
      ProductHistoryId = i.ReferenceUid,
      ProductId = productHistoryLookup[i.ReferenceUid],
      Amount = i.Amount,
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
          EF.Functions.ILike(i.Product.Name, itemName),
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
        EF.Functions.ILike(i.Product.Name, itemName))
      .Select(i => new ShoppingItem(
        i.Product.Name,
        new Quantity($"{i.ProductHistory.ProductFormat.Quantity}"),
        new Price(i.ProductHistory.Price),
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
        EF.Functions.ILike(i.Product.Name, originalItemName))
      .First();

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
          EF.Functions.ILike(i.Product.Name, itemName))
        .First();

      item.DeletedAt = clock.GetCurrentTime();
    }, "Couldn't remove shopping item.");

  public void RecordShoppingList(Guid userUid, ShoppingList shoppingList) {
    Debug.Assert(shoppingList.HasCheckedItems());

    PostgreSqlExceptionMapper.Map(() => {
      List<ShoppingItem> checkedItems = [.. shoppingList.Items.Where(i => i.IsChecked)];

      DateTime now = clock.GetCurrentTime();
      List<PurchaseItemDbEntity> purchaseItems = [];
      foreach (ShoppingItem ci in checkedItems) {
        var history = context.ProductsHistory
          .Where(ph => ph.Product.Name == ci.Name &&
            ph.Price == ci.Price.Value &&
            $"{ph.ProductFormat.Quantity}" == ci.Quantity!.Value)
          .Select(ph => new {
            ph.Id,
            ph.ProductId,
          })
          .First();
        PurchaseItemDbEntity item = new() {
          Amount = 1,
          ProductId = history.ProductId,
          ProductHistoryId = history.Id,
        };
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
}