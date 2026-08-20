using Metaspesa.Domain.Markets;

namespace Metaspesa.Application.Abstractions.Purchasing;

public interface IPurchasePriceSnapshotReader {
  Task<IReadOnlyDictionary<ProductFormatId, PriceSnapshotId>> GetLatestAsync(
    IReadOnlyCollection<ProductFormatId> productFormatIds,
    CancellationToken cancellationToken);
}