
namespace Metaspesa.Domain.Shopping;

public record AShoppingList(
  string? Name,
  IReadOnlyCollection<AShoppingItem> Items
) {
  public AShoppingList OnlyWithCheckedItems() {
    return Items.Where(p => p.IsChecked).ToShoppingList(Name);
  }

  public bool HasCheckedItems() => Items.Any(p => p.IsChecked);
}

public static class ShoppingListExtensions {
  public static AShoppingList ToShoppingList(
    this IEnumerable<AShoppingItem> items, string? name
  ) => new(name, [.. items]);
}