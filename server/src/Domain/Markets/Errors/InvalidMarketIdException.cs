namespace Metaspesa.Domain.Markets.Errors;

public class InvalidMarketIdException : MarketDomainException {
  public InvalidMarketIdException() { }

  public InvalidMarketIdException(Guid value)
    : base("Market.Id.Invalid", $"Market id '{value}' must not be empty.") { }

  public InvalidMarketIdException(string message) : base(message) { }

  public InvalidMarketIdException(string message, Exception innerException)
    : base(message, innerException) { }
}