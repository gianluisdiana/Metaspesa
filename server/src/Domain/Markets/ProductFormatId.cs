using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.Markets;

public readonly record struct ProductFormatId {
  public Guid Value { get; }

  public ProductFormatId(Guid value) {
    if (value == Guid.Empty) {
      throw new InvalidProductFormatIdException(value);
    }

    Value = value;
  }

  public override string ToString() => Value.ToString();
}