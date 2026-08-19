using Metaspesa.Domain.Markets;
using Metaspesa.Domain.SharedKernel;

namespace Metaspesa.Domain.Purchasing;

public sealed class PurchaseItem(
  PriceSnapshotId priceSnapshotId,
  PositiveAmount amount
) {
  public PriceSnapshotId PriceSnapshotId { get; } = priceSnapshotId;
  public PositiveAmount Amount { get; } = amount;
}