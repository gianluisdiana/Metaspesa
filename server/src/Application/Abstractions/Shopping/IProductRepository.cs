using Metaspesa.Domain.Shopping;

namespace Metaspesa.Application.Abstractions.Shopping;

public interface IProductRepository {
  Task<List<Product>> GetRegisteredItemsAsync(
    Guid userUid, CancellationToken cancellationToken);
  void UpdateRegisteredItems(
    Guid userUid, IReadOnlyCollection<ShoppingItem> shoppingItems);
  void RegisterItems(
    Guid userUid, IReadOnlyCollection<ShoppingItem> shoppingItems);
}
