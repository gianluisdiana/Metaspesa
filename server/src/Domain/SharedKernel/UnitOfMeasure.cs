using Metaspesa.Domain.SharedKernel.Errors;

namespace Metaspesa.Domain.SharedKernel;

public readonly record struct UnitOfMeasure {
  private static readonly string[] SupportedUnits = [
    "unit",
    "g",
    "kg",
    "ml",
    "l",
  ];

  public string Value { get; }

  public UnitOfMeasure(string value) {
    string normalizedValue = Normalize(value) ??
      throw InvalidUnitOfMeasureException.ForUnit(value);

    Value = normalizedValue;
  }

  private static string? Normalize(string? value) {
    string trimmedValue = value?.Trim() ?? "";
    return SupportedUnits.FirstOrDefault(
      unit => string.Equals(unit, trimmedValue, StringComparison.OrdinalIgnoreCase));
  }

  public override string ToString() => Value;
}