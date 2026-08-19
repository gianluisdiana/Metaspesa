using Metaspesa.Application.Abstractions.Purchasing;
using Metaspesa.Domain.Markets;
using Microsoft.EntityFrameworkCore;

namespace Metaspesa.Database.Repositories;

internal class PostgreSqlPurchasePriceSnapshotReader(
  MainContext context
) : IPurchasePriceSnapshotReader {
  public async Task<IReadOnlyDictionary<ProductFormatId, PriceSnapshotId>>
    GetLatestAsync(
      IReadOnlyCollection<ProductFormatId> productFormatIds,
      CancellationToken cancellationToken
    ) => await PostgreSqlExceptionMapper.MapAsync<
      IReadOnlyDictionary<ProductFormatId, PriceSnapshotId>>(async () => {
        if (productFormatIds.Count == 0) {
          return new Dictionary<ProductFormatId, PriceSnapshotId>();
        }

        int[] formatIds = [
          .. productFormatIds.Select(id => id.Value).Distinct()
        ];
        var snapshots = await context.PriceSnapshots
          .AsNoTracking()
          .Where(snapshot => formatIds.Contains(snapshot.ProductFormatId))
          .GroupBy(snapshot => snapshot.ProductFormatId)
          .Select(group => group
            .OrderByDescending(snapshot => snapshot.ObservedAt)
            .ThenByDescending(snapshot => snapshot.Id)
            .Select(snapshot => new {
              snapshot.ProductFormatId,
              SnapshotId = snapshot.Id,
            })
            .First())
          .ToListAsync(cancellationToken);

        return snapshots.ToDictionary(
          snapshot => new ProductFormatId(snapshot.ProductFormatId),
          snapshot => new PriceSnapshotId(snapshot.SnapshotId));
      }, "Couldn't resolve purchase price snapshots.");
}