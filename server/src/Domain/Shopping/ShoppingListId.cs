using System.Globalization;
using Metaspesa.Domain.Shopping.Errors;

namespace Metaspesa.Domain.Shopping;

public readonly record struct ShoppingListId {
  public int Value { get; }

  public ShoppingListId(int value) {
    if (value <= 0) {
      throw new InvalidShoppingListIdException(value);
    }

    Value = value;
  }

  public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
