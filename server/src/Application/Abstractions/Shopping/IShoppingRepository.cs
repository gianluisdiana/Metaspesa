using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Abstractions.Shopping;

public interface IShoppingRepository {
  Task<ShoppingList?> GetCurrentShoppingListAsync(
    Guid userUid, CancellationToken cancellationToken);
  void RecordShoppingList(Guid userUid, ShoppingList shoppingList);
}