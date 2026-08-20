namespace Metaspesa.Domain.Markets.Errors;

public class InvalidProductNameException : MarketDomainException {
  public InvalidProductNameException() { }

  public InvalidProductNameException(string? value)
    : base("Market.Product.Name.Invalid", $"Product name '{value}' must not be empty.") { }

  public InvalidProductNameException(string message, Exception innerException)
    : base(message, innerException) { }
}