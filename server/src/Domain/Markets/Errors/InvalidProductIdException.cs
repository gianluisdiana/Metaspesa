namespace Metaspesa.Domain.Markets.Errors;

public class InvalidProductIdException : MarketDomainException {
  public InvalidProductIdException() { }

  public InvalidProductIdException(int value)
    : base("Market.Product.Id.Invalid", $"Product id '{value}' must be greater than zero.") { }

  public InvalidProductIdException(string message) : base(message) { }

  public InvalidProductIdException(string message, Exception innerException)
    : base(message, innerException) { }
}
