namespace Metaspesa.Domain.Markets.Errors;

public class InvalidPriceSnapshotObservedAtException : MarketDomainException {
  public InvalidPriceSnapshotObservedAtException() { }

  public InvalidPriceSnapshotObservedAtException(DateTime value)
    : base(
      "Market.PriceSnapshot.ObservedAt.Invalid",
      $"Price snapshot observation time '{value:O}' must be a non-default UTC value.") { }

  public InvalidPriceSnapshotObservedAtException(string message) : base(message) { }

  public InvalidPriceSnapshotObservedAtException(
    string message, Exception innerException
  ) : base(message, innerException) { }
}