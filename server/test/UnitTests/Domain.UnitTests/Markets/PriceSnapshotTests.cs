using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Markets.Errors;
using Metaspesa.Domain.SharedKernel;

namespace Metaspesa.Domain.UnitTests.Markets;

public static class PriceSnapshotTests {
  [Fact(DisplayName = "Creates immutable price snapshot")]
  public static void PriceSnapshot_Created_WithImmutableObservation() {
    var observedAt = new DateTime(2026, 7, 28, 12, 0, 0, DateTimeKind.Utc);
    var snapshot = new PriceSnapshot(
      new PriceSnapshotId(1),
      new ProductFormatId(2),
      new Money(3.45m),
      observedAt);

    Assert.Equal(new PriceSnapshotId(1), snapshot.Id);
    Assert.Equal(new ProductFormatId(2), snapshot.ProductFormatId);
    Assert.Equal(new Money(3.45m), snapshot.Price);
    Assert.Equal(observedAt, snapshot.ObservedAt);
    Assert.All(
      typeof(PriceSnapshot).GetProperties(),
      property => Assert.False(property.CanWrite));
  }

  [Theory(DisplayName = "Rejects invalid price snapshot observation time")]
  [ClassData<InvalidObservationTimes>]
  public static void PriceSnapshot_ThrowsSpecificException_WhenObservedAtIsInvalid(
    DateTime observedAt
  ) => Assert.Throws<InvalidPriceSnapshotObservedAtException>(
    () => new PriceSnapshot(
      new PriceSnapshotId(1),
      new ProductFormatId(2),
      new Money(3.45m),
      observedAt));

  private class InvalidObservationTimes() : TheoryData<DateTime>(
    default,
    new DateTime(2026, 7, 28, 12, 0, 0, DateTimeKind.Local),
    new DateTime(2026, 7, 28, 12, 0, 0, DateTimeKind.Unspecified)
  );
}