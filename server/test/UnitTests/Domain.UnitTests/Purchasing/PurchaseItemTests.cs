using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Purchasing;
using Metaspesa.Domain.SharedKernel;

namespace Metaspesa.Domain.UnitTests.Purchasing;

public class PurchaseItemTests {
  [Fact(DisplayName = "Creates immutable purchase item from typed values")]
  public void Constructor_ExposesTypedValues() {
    var item = new PurchaseItem(
      new PriceSnapshotId(3), new PositiveAmount(2));

    Assert.Equal(new PriceSnapshotId(3), item.PriceSnapshotId);
    Assert.Equal(new PositiveAmount(2), item.Amount);
    Assert.All(
      typeof(PurchaseItem).GetProperties(),
      property => Assert.False(property.CanWrite));
  }
}
