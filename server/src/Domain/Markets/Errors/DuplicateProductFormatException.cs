namespace Metaspesa.Domain.Markets.Errors;

public class DuplicateProductFormatException : MarketDomainException {
  public DuplicateProductFormatException() { }

  public DuplicateProductFormatException(ProductFormatId id)
    : base(
      "Market.ProductFormat.Duplicate",
      $"Product format '{id}' already belongs to this product.") { }

  public DuplicateProductFormatException(string message) : base(message) { }

  public DuplicateProductFormatException(string message, Exception innerException)
    : base(message, innerException) { }
}