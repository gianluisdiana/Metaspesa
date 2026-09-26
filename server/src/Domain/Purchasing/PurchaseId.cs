using Metaspesa.Domain.Purchasing.Errors;

namespace Metaspesa.Domain.Purchasing;

public readonly record struct PurchaseId {
  public Guid Value { get; }

  public PurchaseId(Guid value) {
    if (value == Guid.Empty) {
      throw new InvalidPurchaseIdException(value);
    }

    Value = value;
  }

  public override string ToString() => Value.ToString();
}