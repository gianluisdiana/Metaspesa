using System.Text.Json.Serialization;

namespace Metaspesa.RestApi.Shopping.GetShoppingList;

/// <summary>Shopping list and its current items.</summary>
/// <param name="Id">Stable shopping list ID.</param>
/// <param name="Name">List name, omitted for a temporary list.</param>
/// <param name="IsTemporary">Whether this is an unnamed temporary list.</param>
/// <param name="Items">Items in the list; empty when it has no items.</param>
internal sealed record ShoppingListResponse(
  int Id,
  [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Name,
  bool IsTemporary,
  IReadOnlyCollection<ShoppingItemResponse> Items);