using System.Diagnostics;

namespace Metaspesa.Domain.Shopping;

public record Price {
  private const float Epsilon = 0.01f;

  public float Value { get; }

  public static readonly Price Empty = new(0);

  public Price(float value) {
    Debug.Assert(PricePolicy.IsValidPrice(value));
    Value = value;
  }

  public bool IsZero() => Value < Epsilon;

  public virtual bool Equals(Price? otherPrice) {
    return otherPrice is not null &&
      Math.Abs(Value - otherPrice.Value) < Epsilon;
  }

  public override int GetHashCode() {
    return HashCode.Combine(Value);
  }
}
