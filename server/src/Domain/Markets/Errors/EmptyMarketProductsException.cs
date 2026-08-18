namespace Metaspesa.Domain.Markets.Errors;

public class EmptyMarketProductsException : MarketDomainException {
  public EmptyMarketProductsException()
    : base(
      "Market.Products.Empty",
      "At least one product must be provided.") { }

  public EmptyMarketProductsException(string message) : base(message) { }

  public EmptyMarketProductsException(
    string message,
    Exception innerException
  ) : base(message, innerException) { }
}
