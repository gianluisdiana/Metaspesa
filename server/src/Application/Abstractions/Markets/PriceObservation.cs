using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Markets.Errors;
using Metaspesa.Domain.SharedKernel;

namespace Metaspesa.Application.Abstractions.Markets;

public sealed record PriceObservation {
  public ProductFormatId ProductFormatId { get; }
  public Money Price { get; }
  public DateTime ObservedAt { get; }

  public PriceObservation(
    ProductFormatId productFormatId, Money price, DateTime observedAt
  ) {
    if (observedAt == default || observedAt.Kind != DateTimeKind.Utc) {
      throw new InvalidPriceSnapshotObservedAtException(observedAt);
    }

    ProductFormatId = productFormatId;
    Price = price;
    ObservedAt = observedAt;
  }
}
