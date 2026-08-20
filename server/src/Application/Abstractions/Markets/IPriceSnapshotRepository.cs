using Metaspesa.Domain.Markets;

namespace Metaspesa.Application.Abstractions.Markets;

public interface IPriceSnapshotRepository {
  Task<PriceSnapshot?> GetByIdAsync(
    PriceSnapshotId snapshotId, CancellationToken cancellationToken);
  Task AppendAsync(
    IReadOnlyCollection<PriceObservation> observations,
    CancellationToken cancellationToken);
  Task DeleteForMarketsAsync(
    IReadOnlyCollection<MarketName> marketNames,
    DateTime observedAt,
    CancellationToken cancellationToken);
}