using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.Markets;

public readonly record struct ProductName {
  public string Value { get; }

  public ProductName(string value) {
    if (string.IsNullOrWhiteSpace(value)) {
      throw new InvalidProductNameException(value);
    }

    Value = value.Trim();
  }

  public override string ToString() => Value;
}