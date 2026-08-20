using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.Markets;

public readonly record struct BrandName {
  public string Value { get; }

  public BrandName(string value) {
    if (string.IsNullOrWhiteSpace(value)) {
      throw new InvalidBrandNameException(value);
    }

    Value = value.Trim();
  }

  public override string ToString() => Value;
}