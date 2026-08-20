namespace Metaspesa.Domain.Markets.Errors;

public class ProductFormatNotFoundException : MarketDomainException {
  public ProductFormatNotFoundException() { }

  public ProductFormatNotFoundException(ProductFormatId id)
    : base(
      "Market.ProductFormat.NotFound",
      $"Product format '{id}' does not belong to this product.") { }

  public ProductFormatNotFoundException(string message) : base(message) { }

  public ProductFormatNotFoundException(string message, Exception innerException)
    : base(message, innerException) { }
}