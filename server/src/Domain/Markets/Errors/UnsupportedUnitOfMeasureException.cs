namespace Metaspesa.Domain.Markets.Errors;

public class UnsupportedUnitOfMeasureException : MarketDomainException {
  public UnsupportedUnitOfMeasureException() { }

  public UnsupportedUnitOfMeasureException(string value)
    : base(
      "Market.Product.UnitOfMeasure.Unsupported",
      $"Unit of measure '{value}' is not supported.") { }

  public UnsupportedUnitOfMeasureException(
    string message,
    Exception innerException
  ) : base(message, innerException) { }
}
