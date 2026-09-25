using Metaspesa.Domain.Identity;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Abstractions.Shopping;

public interface IShoppingListRepository {
  Task<IReadOnlyCollection<ShoppingList>> GetByOwnerAsync(
    UserId ownerId, CancellationToken cancellationToken);
  Task<ShoppingList?> GetAsync(
    UserId ownerId,
    ShoppingListId id,
    CancellationToken cancellationToken);
  Task<bool> ExistsAsync(
    Guid ownerId, string? name, CancellationToken cancellationToken);
  Task<int> AddAsync(ShoppingList shoppingList, CancellationToken cancellationToken);
  Task UpdateAsync(
    ShoppingList shoppingList, CancellationToken cancellationToken);
}