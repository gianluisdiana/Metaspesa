namespace Metaspesa.RestApi.Markets.GetProducts;

/// <summary>Monetary amount and currency.</summary>
/// <param name="Amount">Price in the stated currency.</param>
/// <param name="Currency">Currency code of the price, such as EUR.</param>
internal sealed record MoneyResponse(decimal Amount, string Currency);