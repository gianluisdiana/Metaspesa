namespace Metaspesa.Domain.Markets.Errors;

public class InvalidProductFormatIdException : MarketDomainException {
  public InvalidProductFormatIdException() { }

  public InvalidProductFormatIdException(int value)
    : base(
      "Market.ProductFormat.Id.Invalid",
      $"Product format id '{value}' must be greater than zero.") { }

  public InvalidProductFormatIdException(string message) : base(message) { }

  public InvalidProductFormatIdException(string message, Exception innerException)
    : base(message, innerException) { }
}