namespace Metaspesa.RestApi.Shopping.GetShoppingList;

/// <summary>Quantity sold in one product format.</summary>
/// <param name="Amount">Numeric quantity of the format.</param>
/// <param name="Unit">Unit of measure, such as kg, g, or l.</param>
internal sealed record ShoppingQuantityResponse(decimal Amount, string Unit);