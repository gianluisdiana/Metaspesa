using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Abstractions.Shopping;

public interface IShoppingListRepository {
  Task<IReadOnlyCollection<ShoppingList>> GetByOwnerAsync(
    UserId ownerId, CancellationToken cancellationToken);
  Task<ShoppingList?> GetAsync(
    UserId ownerId,
    ShoppingListName? name,
    CancellationToken cancellationToken);
  Task<bool> ExistsAsync(
    UserId ownerId,
    ShoppingListName? name,
    CancellationToken cancellationToken);
  void Add(ShoppingList shoppingList);
  Task UpdateAsync(
    ShoppingList shoppingList, CancellationToken cancellationToken);
}