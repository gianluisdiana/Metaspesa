using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Metaspesa.Domain.Shopping;

public record ShoppingList(
  string? Name,
  IReadOnlyCollection<ShoppingItem> Items
) : IEnumerable<ShoppingItem> {

  [MemberNotNullWhen(true, nameof(Name))]
  public bool IsNamed => !string.IsNullOrWhiteSpace(Name);

  public ShoppingList Intersecting(
    IReadOnlyCollection<Product> products
  ) => this with {
    Items = [.. Items.Where(i => products.Any(p => p.NormalizedName == i.NormalizedName))]
  };

  public ShoppingList Without(
    IReadOnlyCollection<Product> products
  ) => this with {
    Items = [.. Items.Where(i => products.All(p => p.NormalizedName != i.NormalizedName))]
  };

  public IEnumerator<ShoppingItem> GetEnumerator() => Items.GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
