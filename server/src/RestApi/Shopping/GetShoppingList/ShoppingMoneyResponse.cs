namespace Metaspesa.RestApi.Shopping.GetShoppingList;

/// <summary>Latest observed price for one product format.</summary>
/// <param name="Amount">Price in the stated currency.</param>
/// <param name="Currency">Currency code of the price, such as EUR.</param>
internal sealed record ShoppingMoneyResponse(decimal Amount, string Currency);