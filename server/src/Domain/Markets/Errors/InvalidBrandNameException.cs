namespace Metaspesa.Domain.Markets.Errors;

public class InvalidBrandNameException : MarketDomainException {
  public InvalidBrandNameException() { }

  public InvalidBrandNameException(string? value)
    : base("Market.Brand.Name.Invalid", $"Brand name '{value}' must not be empty.") { }

  public InvalidBrandNameException(string message, Exception innerException)
    : base(message, innerException) { }
}