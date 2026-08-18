using System.Globalization;
using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.Markets;

public readonly record struct MarketId {
  public int Value { get; }

  public MarketId(int value) {
    if (value <= 0) {
      throw new InvalidMarketIdException(value);
    }

    Value = value;
  }

  public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
