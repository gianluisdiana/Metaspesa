using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.Markets;

public readonly record struct MarketName {
  public string Value { get; }

  public MarketName(string value) {
    if (string.IsNullOrWhiteSpace(value)) {
      throw new InvalidMarketNameException(value);
    }

    Value = value.Trim();
  }

  public override string ToString() => Value;
}