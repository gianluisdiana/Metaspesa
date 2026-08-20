using Metaspesa.Domain.Purchasing;
using Metaspesa.Domain.Purchasing.Errors;

namespace Metaspesa.Domain.UnitTests.Purchasing;

public class PurchaseIdTests {
  [Fact(DisplayName = "Creates and compares purchase IDs by value")]
  public void Constructor_CreatesValueObject_WhenIdIsPositive() {
    Assert.Equal(new PurchaseId(1), new PurchaseId(1));
  }

  [Theory(DisplayName = "Rejects invalid purchase ID")]
  [InlineData(0)]
  [InlineData(-1)]
  public void Constructor_ThrowsExactException_WhenIdIsInvalid(int value) {
    Assert.Throws<InvalidPurchaseIdException>(() => new PurchaseId(value));
  }
}