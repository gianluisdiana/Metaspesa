using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.Markets;

public readonly record struct MarketId {
  public Guid Value { get; }

  public MarketId(Guid value) {
    if (value == Guid.Empty) {
      throw new InvalidMarketIdException(value);
    }

    Value = value;
  }

  public override string ToString() => Value.ToString();
}