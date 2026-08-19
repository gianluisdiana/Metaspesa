using System.Globalization;
using Metaspesa.Domain.Purchasing.Errors;

namespace Metaspesa.Domain.Purchasing;

public readonly record struct PurchaseId {
  public int Value { get; }

  public PurchaseId(int value) {
    if (value <= 0) {
      throw new InvalidPurchaseIdException(value);
    }

    Value = value;
  }

  public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}