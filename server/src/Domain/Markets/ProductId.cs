using System.Globalization;
using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.Markets;

public readonly record struct ProductId {
  public int Value { get; }

  public ProductId(int value) {
    if (value <= 0) {
      throw new InvalidProductIdException(value);
    }

    Value = value;
  }

  public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
