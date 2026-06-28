using Metaspesa.Application.Abstractions.Shopping;
using Microsoft.EntityFrameworkCore;

namespace Metaspesa.Database.Repositories;

internal partial class PostgreSqlProductRepository(
  MainContext context
) : IProductRepository {
  public Task<bool> CheckProductExistsAsync(
    long referenceUid, CancellationToken cancellationToken
  ) => PostgreSqlExceptionMapper.MapAsync(async () =>
      await context.ProductFormats.AnyAsync(
        p => p.Id == referenceUid, cancellationToken),
    "Couldn't check if product exists.");
}