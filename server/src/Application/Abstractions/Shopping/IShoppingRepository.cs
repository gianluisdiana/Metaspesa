using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Abstractions.Shopping;

public interface IShoppingRepository {
  Task<ShoppingList?> GetCurrentShoppingListAsync(
    Guid userUid, CancellationToken cancellationToken);
  Task<bool> CheckShoppingListExistAsync(
    Guid userUid, string? shoppingListName, CancellationToken cancellationToken);
  void CreateShoppingList(Guid userUid, string? name);
  void AddItemsToList(Guid userUid, string? listName, IReadOnlyCollection<ShoppingItem> items);
  Task<bool> CheckItemExistsAsync(Guid userUid, string? listName, string itemName, CancellationToken cancellationToken);
  Task<ShoppingItem?> GetItemAsync(Guid userUid, string? listName, string itemName, CancellationToken cancellationToken);
  void UpdateItem(Guid userUid, string? listName, string originalItemName, ShoppingItem update);
  void RecordShoppingList(Guid userUid, ShoppingList shoppingList);
}