using System.Globalization;
using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.Markets;

public readonly record struct ProductFormatId {
  public int Value { get; }

  public ProductFormatId(int value) {
    if (value <= 0) {
      throw new InvalidProductFormatIdException(value);
    }

    Value = value;
  }

  public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}