using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Abstractions.Shopping;

public interface IShoppingRepository {
  Task<List<ShoppingList>> GetShoppingListSummariesAsync(
    Guid userUid, CancellationToken cancellationToken);
  Task<ShoppingList?> GetShoppingListAsync(
    Guid userUid, string? shoppingListName, CancellationToken cancellationToken);
  Task<bool> CheckShoppingListExistAsync(
    Guid userUid, string? shoppingListName, CancellationToken cancellationToken);
  void CreateShoppingList(Guid userUid, string? name);
  void UpdateShoppingListName(Guid userUid, string? listName, string? newName);
  void AddItemsToList(Guid userUid, string? listName, IReadOnlyCollection<AShoppingItem> items);
  Task<bool> CheckItemExistsAsync(
    Guid userUid, string? listName, int referenceUid, CancellationToken cancellationToken);
  Task<AShoppingItem?> GetItemAsync(
    Guid userUid, string? listName, int referenceUid, CancellationToken cancellationToken);
  void UpdateItem(Guid userUid, string? listName, AShoppingItem update);
  void RemoveItem(Guid userUid, string? listName, int referenceUid);
  void RecordShoppingList(Guid userUid, ShoppingList shoppingList);
}