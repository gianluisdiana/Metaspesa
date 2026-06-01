using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Database.Repositories;

internal partial class PostgreSqlProductRepository(
) : IProductRepository {
  public Task<List<Product>> GetRegisteredItemsAsync(
    Guid userUid, CancellationToken cancellationToken
  ) => Task.FromResult(new List<Product>());

  public void RegisterItems(
    Guid userUid, IReadOnlyCollection<ShoppingItem> shoppingItems
  ) => PostgreSqlExceptionMapper.Map(() => {
  }, "Couldn't register items.");

  public void UpdateRegisteredItems(
    Guid userUid, IReadOnlyCollection<ShoppingItem> shoppingItems
  ) => PostgreSqlExceptionMapper.Map(() => {
  }, "Couldn't update registered items.");
}