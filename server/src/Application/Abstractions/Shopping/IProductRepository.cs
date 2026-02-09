using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Abstractions.Shopping;

public interface IProductRepository {
  Task<List<RegisteredItem>> GetRegisteredItemsAsync(
    Guid userUid, CancellationToken cancellationToken);
  void UpdateItems(
    Guid userUid, IReadOnlyCollection<ShoppingItem> shoppingItems);
  void RegisterItems(
    Guid userUid, IReadOnlyCollection<ShoppingItem> shoppingItems);
}
