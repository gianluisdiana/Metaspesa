using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.Shopping.Errors;

namespace Metaspesa.Domain.Shopping;

public sealed class ShoppingList {
  private readonly List<UserId> _ownerIds;
  private readonly List<ShoppingItem> _items;

  public ShoppingListId? Id { get; }
  public ShoppingListName? Name { get; private set; }
  public DateTime? DeletedAt { get; }
  public IReadOnlyCollection<UserId> OwnerIds => _ownerIds.AsReadOnly();
  public IReadOnlyCollection<ShoppingItem> Items => _items.AsReadOnly();

  public bool IsTemporary => Name is null;

  private ShoppingList(
    ShoppingListId? id,
    IEnumerable<UserId> ownerIds,
    ShoppingListName? name,
    IEnumerable<ShoppingItem>? items,
    DateTime? deletedAt
  ) {
    _ownerIds = [.. ownerIds];
    if (_ownerIds.Count == 0) {
      throw new MissingShoppingListOwnerException();
    }

    UserId? duplicateOwner = _ownerIds
      .GroupBy(ownerId => ownerId)
      .Where(group => group.Count() > 1)
      .Select(group => (UserId?)group.Key)
      .FirstOrDefault();
    if (duplicateOwner.HasValue) {
      throw new DuplicateShoppingListOwnerException(duplicateOwner.Value);
    }

    Id = id;
    Name = name;
    DeletedAt = deletedAt;
    _items = items?.ToList() ?? [];

    ProductFormatId? duplicateItem = _items
      .GroupBy(item => item.ProductFormatId)
      .Where(group => group.Count() > 1)
      .Select(group => (ProductFormatId?)group.Key)
      .FirstOrDefault();
    if (duplicateItem.HasValue) {
      throw new DuplicateShoppingItemException(duplicateItem.Value);
    }
  }

  public static ShoppingList Create(
    Guid primitiveOwnerId,
    string? primitiveName
  ) {
    var ownerId = new UserId(primitiveOwnerId);
    ShoppingListName? name = string.IsNullOrWhiteSpace(primitiveName)
      ? null : new ShoppingListName(primitiveName);

    return new ShoppingList(null, [ownerId], name, [], null);
  }

  public static ShoppingList Create(UserId ownerId, ShoppingListName? name) =>
    new(null, [ownerId], name, null, null);

  public static ShoppingList Rehydrate(
    ShoppingListId id,
    IEnumerable<UserId> ownerIds,
    ShoppingListName? name,
    DateTime? deletedAt,
    IEnumerable<ShoppingItem> items
  ) => new(id, ownerIds, name, items, deletedAt);

  public void Rename(ShoppingListName name) {
    Name = name;
  }

  public void AddItem(
    ProductFormatId productFormatId, PositiveAmount amount, bool isChecked
  ) => AddItems([new ShoppingItem(productFormatId, amount, isChecked)]);

  public void AddItems(IEnumerable<ShoppingItem> items) {
    var additions = items.ToList();
    ProductFormatId? duplicateId = _items
      .Select(item => item.ProductFormatId)
      .Concat(additions.Select(item => item.ProductFormatId))
      .GroupBy(id => id)
      .Where(group => group.Count() > 1)
      .Select(group => (ProductFormatId?)group.Key)
      .FirstOrDefault();
    if (duplicateId.HasValue) {
      throw new DuplicateShoppingItemException(duplicateId.Value);
    }

    _items.AddRange(additions);
  }

  public void UpdateItem(
    ProductFormatId productFormatId, PositiveAmount? amount, bool? isChecked
  ) {
    if (!amount.HasValue && !isChecked.HasValue) {
      throw new EmptyShoppingItemUpdateException();
    }

    ShoppingItem item = FindItem(productFormatId);
    item.Update(amount, isChecked);
  }

  public void RemoveItem(ProductFormatId productFormatId) {
    ShoppingItem item = FindItem(productFormatId);
    _items.Remove(item);
  }

  public IReadOnlyCollection<ShoppingItem> CheckedItems() =>
    _items.Where(item => item.IsChecked).ToList().AsReadOnly();

  public void ResetCheckedItems() {
    foreach (ShoppingItem item in _items.Where(item => item.IsChecked)) {
      item.Update(null, false);
    }
  }

  private ShoppingItem FindItem(ProductFormatId productFormatId) =>
    _items.FirstOrDefault(item => item.ProductFormatId == productFormatId) ??
    throw new ShoppingItemNotFoundException(productFormatId);
}