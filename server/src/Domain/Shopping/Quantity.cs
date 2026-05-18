using System.Diagnostics;

namespace Metaspesa.Domain.Shopping;

public record Quantity {
  public const int MaximumLength = 50;

  public string Value { get; }

  public Quantity(string value) {
    Debug.Assert(IsValid(value));

    Value = value;
  }

  private static bool IsValid(string? value) =>
    value is null || value.Length <= MaximumLength;

  public static Quantity? FromNullable(string? value) =>
    value is null ? null : new Quantity(value);

  public static implicit operator Quantity(string value) => new(value);

  public static implicit operator string(Quantity quantity) {
    Debug.Assert(quantity is not null);

    return quantity.Value;
  }

  public override string ToString() => Value;
}
