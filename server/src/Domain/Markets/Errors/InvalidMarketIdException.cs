namespace Metaspesa.Domain.Markets.Errors;

public class InvalidMarketIdException : MarketDomainException {
  public InvalidMarketIdException() { }

  public InvalidMarketIdException(int value)
    : base("Market.Id.Invalid", $"Market id '{value}' must be greater than zero.") { }

  public InvalidMarketIdException(string message) : base(message) { }

  public InvalidMarketIdException(string message, Exception innerException)
    : base(message, innerException) { }
}
