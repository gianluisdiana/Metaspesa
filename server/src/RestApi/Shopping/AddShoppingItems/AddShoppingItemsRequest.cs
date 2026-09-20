namespace Metaspesa.RestApi.Shopping.AddShoppingItems;

/// <summary>Items to add to a shopping list.</summary>
/// <param name="Items">Non-empty array of items with distinct product format IDs.</param>
internal sealed record AddShoppingItemsRequest(ShoppingItemRequest[]? Items);