namespace Metaspesa.Domain.Markets.Errors;

public class DuplicateMarketProductException : MarketDomainException {
  public DuplicateMarketProductException() { }

  public DuplicateMarketProductException(
    string? productName,
    string? marketName,
    string? brandName
  ) : base(
    "Market.Product.Duplicate",
    "Each product must be unique in name, market and brand combination. " +
    $"Repeated product: name '{productName}', market '{marketName}', " +
    $"brand '{brandName}'.") { }

  public DuplicateMarketProductException(string message) : base(message) { }

  public DuplicateMarketProductException(
    string message,
    Exception innerException
  ) : base(message, innerException) { }
}