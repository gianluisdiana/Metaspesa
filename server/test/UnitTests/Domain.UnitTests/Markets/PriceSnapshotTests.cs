using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Markets.Errors;
using Metaspesa.Domain.SharedKernel;

namespace Metaspesa.Domain.UnitTests.Markets;

public static class PriceSnapshotTests {
  [Fact(DisplayName = "Has same price when money values match")]
  public static void HasSamePrice_ReturnsTrue_WhenMoneyValuesMatch() {
    var snapshot = new PriceSnapshot(
      new PriceSnapshotId(Guid.Parse("00000000-0000-7000-8000-000000000001")),
      new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002")),
      new Money(3.45m),
      new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc));

    bool hasSamePrice = snapshot.HasSamePrice(new Money(3.45m));

    Assert.True(hasSamePrice);
  }

  [Fact(DisplayName = "Has same price when money values round equally")]
  public static void HasSamePrice_ReturnsTrue_WhenMoneyValuesRoundEqually() {
    var snapshot = new PriceSnapshot(
      new PriceSnapshotId(Guid.Parse("00000000-0000-7000-8000-000000000001")),
      new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002")),
      new Money(3.45m),
      new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc));

    bool hasSamePrice = snapshot.HasSamePrice(new Money(3.451m));

    Assert.True(hasSamePrice);
  }

  [Fact(DisplayName = "Does not have same price when money value increases")]
  public static void HasSamePrice_ReturnsFalse_WhenMoneyValueIncreases() {
    var snapshot = new PriceSnapshot(
      new PriceSnapshotId(Guid.Parse("00000000-0000-7000-8000-000000000001")),
      new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002")),
      new Money(3.45m),
      new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc));

    bool hasSamePrice = snapshot.HasSamePrice(new Money(3.46m));

    Assert.False(hasSamePrice);
  }

  [Fact(DisplayName = "Does not have same price when money value decreases")]
  public static void HasSamePrice_ReturnsFalse_WhenMoneyValueDecreases() {
    var snapshot = new PriceSnapshot(
      new PriceSnapshotId(Guid.Parse("00000000-0000-7000-8000-000000000001")),
      new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002")),
      new Money(3.45m),
      new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc));

    bool hasSamePrice = snapshot.HasSamePrice(new Money(3.44m));

    Assert.False(hasSamePrice);
  }

  [Fact(DisplayName = "Creates immutable price snapshot")]
  public static void PriceSnapshot_Created_WithImmutableObservation() {
    var observedAt = new DateTime(2026, 7, 28, 12, 0, 0, DateTimeKind.Utc);
    var snapshot = new PriceSnapshot(
      new PriceSnapshotId(Guid.Parse("00000000-0000-7000-8000-000000000001")),
      new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002")),
      new Money(3.45m),
      observedAt);

    Assert.Equal(new PriceSnapshotId(Guid.Parse("00000000-0000-7000-8000-000000000001")), snapshot.Id);
    Assert.Equal(new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002")), snapshot.ProductFormatId);
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
      new PriceSnapshotId(Guid.Parse("00000000-0000-7000-8000-000000000001")),
      new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000002")),
      new Money(3.45m),
      observedAt));

  private class InvalidObservationTimes() : TheoryData<DateTime>(
    default,
    new DateTime(2026, 7, 28, 12, 0, 0, DateTimeKind.Local),
    new DateTime(2026, 7, 28, 12, 0, 0, DateTimeKind.Unspecified)
  );

}