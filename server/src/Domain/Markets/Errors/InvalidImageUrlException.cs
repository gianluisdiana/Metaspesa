namespace Metaspesa.Domain.Markets.Errors;

public class InvalidImageUrlException : MarketDomainException {
  public InvalidImageUrlException() { }

  public InvalidImageUrlException(Uri? value)
    : base("Market.ImageUrl.Invalid", $"Image URL '{value}' must be absolute.") { }

  public InvalidImageUrlException(string message) : base(message) { }

  public InvalidImageUrlException(string message, Exception innerException)
    : base(message, innerException) { }
}