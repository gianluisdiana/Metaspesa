namespace Metaspesa.Domain.Markets.Errors;

public class InvalidMarketNameException : MarketDomainException {
  public InvalidMarketNameException() { }

  public InvalidMarketNameException(string? value)
    : base("Market.Name.Invalid", $"Market name '{value}' must not be empty.") { }

  public InvalidMarketNameException(string message, Exception innerException)
    : base(message, innerException) { }
}