using System.Globalization;
using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.Markets;

public readonly record struct PriceSnapshotId {
  public int Value { get; }

  public PriceSnapshotId(int value) {
    if (value <= 0) {
      throw new InvalidPriceSnapshotIdException(value);
    }

    Value = value;
  }

  public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}