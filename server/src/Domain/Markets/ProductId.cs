using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.Markets;

public readonly record struct ProductId {
  public Guid Value { get; }

  public ProductId(Guid value) {
    if (value == Guid.Empty) {
      throw new InvalidProductIdException(value);
    }

    Value = value;
  }

  public override string ToString() => Value.ToString();
}