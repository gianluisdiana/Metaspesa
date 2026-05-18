using System.Diagnostics;

namespace Metaspesa.Domain.Shopping;

public record Price {
  private const decimal Epsilon = 0.01m;

  public decimal Value { get; }

  public static readonly Price Empty = new(0);

  public Price(decimal value) {
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