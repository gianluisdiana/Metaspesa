namespace Metaspesa.Domain.Markets.Errors;

public class InvalidPriceSnapshotIdException : MarketDomainException {
  public InvalidPriceSnapshotIdException() { }

  public InvalidPriceSnapshotIdException(Guid value)
    : base(
      "Market.PriceSnapshot.Id.Invalid",
      $"Price snapshot id '{value}' must not be empty.") { }

  public InvalidPriceSnapshotIdException(string message) : base(message) { }

  public InvalidPriceSnapshotIdException(string message, Exception innerException)
    : base(message, innerException) { }
}