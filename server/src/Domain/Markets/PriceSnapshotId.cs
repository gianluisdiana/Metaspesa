using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.Markets;

public readonly record struct PriceSnapshotId {
  public Guid Value { get; }

  public PriceSnapshotId(Guid value) {
    if (value == Guid.Empty) {
      throw new InvalidPriceSnapshotIdException(value);
    }

    Value = value;
  }

  public override string ToString() => Value.ToString();
}