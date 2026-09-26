using System.Text.Json.Serialization;

namespace Metaspesa.RestApi.Shopping.GetShoppingLists;

/// <summary>Shopping list identity and display information.</summary>
/// <param name="Id">Stable shopping list ID.</param>
/// <param name="Name">List name, omitted for a temporary list.</param>
/// <param name="IsTemporary">Whether this is the shopper's unnamed temporary list.</param>
internal sealed record ShoppingListSummaryResponse(
  Guid Id,
  [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Name,
  bool IsTemporary);