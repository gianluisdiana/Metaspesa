namespace Metaspesa.Domain.Markets.Errors;

public class InvalidMarketProductsRegisteredAtException
  : MarketDomainException {
  public InvalidMarketProductsRegisteredAtException() { }

  public InvalidMarketProductsRegisteredAtException(DateOnly value)
    : base(
      "Market.RegisteredAt.TooOld",
      $"RegisteredAt '{value:yyyy-MM-dd}' must be on or after January 1, 2023.") { }

  public InvalidMarketProductsRegisteredAtException(
    string message
  ) : base(message) { }

  public InvalidMarketProductsRegisteredAtException(
    string message,
    Exception innerException
  ) : base(message, innerException) { }
}
