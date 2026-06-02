using Metaspesa.Application.Abstractions.Shopping;
using Metaspesa.Domain.Shopping;
using Microsoft.EntityFrameworkCore;

namespace Metaspesa.Database.Repositories;

internal partial class PostgreSqlProductRepository(
  MainContext context
) : IProductRepository {
  public Task<bool> CheckProductExistsAsync(
    long referenceUid, CancellationToken cancellationToken
  ) => PostgreSqlExceptionMapper.MapAsync(async () =>
      await context.ProductsHistory.AnyAsync(
        p => p.Id == referenceUid, cancellationToken),
    "Couldn't check if product exists.");

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