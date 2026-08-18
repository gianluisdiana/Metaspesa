using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Abstractions.Shopping;

public interface IShoppingPurchaseRepository {
  Task<ShoppingList?> GetAsync(
    UserId ownerId,
    ShoppingListName? name,
    CancellationToken cancellationToken);
  void Record(UserId ownerId, ShoppingList shoppingList);
  void Reset(UserId ownerId, ShoppingListName? name);
}
