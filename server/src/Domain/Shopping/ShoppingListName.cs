using Metaspesa.Domain.Shopping.Errors;

namespace Metaspesa.Domain.Shopping;

public readonly record struct ShoppingListName {
  public string Value { get; }

  public ShoppingListName(string value) {
    if (string.IsNullOrWhiteSpace(value)) {
      throw new InvalidShoppingListNameException();
    }

    Value = value.Trim();
  }

  public override string ToString() => Value;
}