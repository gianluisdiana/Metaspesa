namespace Metaspesa.Domain.Markets.Errors;

public class DuplicateMarketProductException : MarketDomainException {
  public DuplicateMarketProductException() { }

  public DuplicateMarketProductException(
    string? productName,
    string? marketName,
    string? brandName,
    float quantity,
    string? unitOfMeasure
  ) : base(
    "Market.Product.Duplicate",
    "Each product must be unique in name, market, brand, quantity and unit of measure combination. " +
    $"Repeated product: name '{productName}', market '{marketName}', " +
    $"brand '{brandName}', quantity '{quantity}', unitOfMeasure '{unitOfMeasure}'.") { }

  public DuplicateMarketProductException(string message) : base(message) { }

  public DuplicateMarketProductException(
    string message,
    Exception innerException
  ) : base(message, innerException) { }
}