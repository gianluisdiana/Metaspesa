using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.UnitTests.Markets;

public static class PriceSnapshotIdTests {
  [Fact(DisplayName = "Creates and compares price snapshots ids by value")]
  public static void PriceSnapshotId_CreatedAndEqual_WhenValueIsPositive() {
    var id = new PriceSnapshotId(1);

    Assert.Equal(1, id.Value);
    Assert.Equal(id, new PriceSnapshotId(1));
  }

  [Theory(DisplayName = "Rejects non-positive price snapshot ids")]
  [InlineData(0)]
  [InlineData(-1)]
  public static void PriceSnapshotId_ThrowsSpecificException_WhenValueIsInvalid(int value) =>
    Assert.Throws<InvalidPriceSnapshotIdException>(() => new PriceSnapshotId(value));
}
