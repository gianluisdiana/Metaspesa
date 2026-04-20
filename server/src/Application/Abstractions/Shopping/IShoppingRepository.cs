using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Abstractions.Shopping;

public interface IShoppingRepository {
  Task<ShoppingList?> GetCurrentShoppingListAsync(
    Guid userUid, CancellationToken cancellationToken);
  Task<bool> CheckShoppingListExistAsync(
    Guid userUid, string? shoppingListName, CancellationToken cancellationToken);
  void CreateShoppingList(Guid userUid, string? name);
  void RecordShoppingList(Guid userUid, ShoppingList shoppingList);
}