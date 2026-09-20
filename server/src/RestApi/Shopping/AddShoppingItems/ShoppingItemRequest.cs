namespace Metaspesa.RestApi.Shopping.AddShoppingItems;

/// <summary>One product format to add to a shopping list.</summary>
/// <param name="ProductFormatId">ID of an existing product format.</param>
/// <param name="Amount">Positive number of units to buy.</param>
/// <param name="Checked">Whether the item is marked for checkout.</param>
internal sealed record ShoppingItemRequest(int ProductFormatId, int Amount, bool Checked);