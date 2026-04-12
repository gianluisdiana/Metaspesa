using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Metaspesa.Domain.Shopping;

public record ShoppingList(
  string? Name,
  IReadOnlyCollection<ShoppingItem> Items
) : IReadOnlyCollection<ShoppingItem> {

  [MemberNotNullWhen(false, nameof(Name))]
  public bool IsTemporary() => string.IsNullOrWhiteSpace(Name);

  public ShoppingList OnlyWithPriceChangedItems(
    IReadOnlyCollection<Product> products
  ) {
    List<ShoppingItem> priceChangedItems = [];

    foreach (ShoppingItem item in Items) {
      Product? matchingProduct = products.FirstOrDefault(p => p == item);
      if (matchingProduct is not null && !matchingProduct.HasSamePrice(item)) {
        priceChangedItems.Add(item);
      }
    }

    return priceChangedItems.ToShoppingList(Name);
  }

  public ShoppingList Without(
    IReadOnlyCollection<Product> products
  ) {
    return Items.Where(i => products.All(p => p != i))
      .ToShoppingList(Name);
  }

  public bool HasCheckedItems() => Items.Any(p => p.IsChecked);
  public ShoppingList OnlyWithCheckedItems() {
    return Items.Where(p => p.IsChecked).ToShoppingList(Name);
  }

  public int Count => Items.Count;
  public IEnumerator<ShoppingItem> GetEnumerator() => Items.GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public static class ShoppingListExtensions {
  public static ShoppingList ToShoppingList(
    this IEnumerable<ShoppingItem> items, string? name
  ) => new(name, [.. items]);
}