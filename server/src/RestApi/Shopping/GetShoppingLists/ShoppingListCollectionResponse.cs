namespace Metaspesa.RestApi.Shopping.GetShoppingLists;

/// <summary>Shopping lists owned by the authenticated shopper.</summary>
/// <param name="Items">List summaries; empty when the shopper owns no lists.</param>
internal sealed record ShoppingListCollectionResponse(
  IReadOnlyCollection<ShoppingListSummaryResponse> Items);