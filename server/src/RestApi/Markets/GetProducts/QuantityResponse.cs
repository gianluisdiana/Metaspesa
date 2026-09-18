namespace Metaspesa.RestApi.Markets.GetProducts;

/// <summary>Quantity associated with one product format.</summary>
/// <param name="Amount">Numeric quantity of the format.</param>
/// <param name="Unit">Unit of measure, such as kg, g, or l.</param>
internal sealed record QuantityResponse(decimal Amount, string Unit);