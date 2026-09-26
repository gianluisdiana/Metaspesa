namespace Metaspesa.Domain.Markets.Errors;

public class InvalidProductFormatIdException : MarketDomainException {
  public InvalidProductFormatIdException() { }

  public InvalidProductFormatIdException(Guid value)
    : base(
      "Market.ProductFormat.Id.Invalid",
      $"Product format id '{value}' must not be empty.") { }

  public InvalidProductFormatIdException(string message) : base(message) { }

  public InvalidProductFormatIdException(string message, Exception innerException)
    : base(message, innerException) { }
}