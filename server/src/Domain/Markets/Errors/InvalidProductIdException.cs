namespace Metaspesa.Domain.Markets.Errors;

public class InvalidProductIdException : MarketDomainException {
  public InvalidProductIdException() { }

  public InvalidProductIdException(Guid value)
    : base("Market.Product.Id.Invalid", $"Product id '{value}' must not be empty.") { }

  public InvalidProductIdException(string message) : base(message) { }

  public InvalidProductIdException(string message, Exception innerException)
    : base(message, innerException) { }
}