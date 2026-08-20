using Metaspesa.Domain.Markets.Errors;
using Metaspesa.Domain.SharedKernel;

namespace Metaspesa.Domain.Markets;

public sealed class PriceSnapshot {
  public PriceSnapshotId Id { get; }
  public ProductFormatId ProductFormatId { get; }
  public Money Price { get; }
  public DateTime ObservedAt { get; }

  public PriceSnapshot(
    PriceSnapshotId id,
    ProductFormatId productFormatId,
    Money price,
    DateTime observedAt
  ) {
    if (observedAt == default || observedAt.Kind != DateTimeKind.Utc) {
      throw new InvalidPriceSnapshotObservedAtException(observedAt);
    }

    Id = id;
    ProductFormatId = productFormatId;
    Price = price;
    ObservedAt = observedAt;
  }
}