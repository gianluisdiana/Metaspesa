using System.Diagnostics;

namespace Metaspesa.Domain.Markets;

public record AQuantity {
  public float Value { get; }
  public string UnitOfMeasure { get; }

  public AQuantity(float value, string unitOfMeasure) {
    Debug.Assert(value > 0);

    Value = value;
    UnitOfMeasure = unitOfMeasure;
  }
}