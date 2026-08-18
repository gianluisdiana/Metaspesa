using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.UnitTests.Markets;

public static class ProductFormatIdTests {
  [Theory(DisplayName = "Rejects non-positive product format ids")]
  [InlineData(0)]
  [InlineData(-1)]
  public static void ProductFormatId_ThrowsSpecificException_WhenValueIsInvalid(int value) =>
    Assert.Throws<InvalidProductFormatIdException>(() => new ProductFormatId(value));

  [Fact(DisplayName = "Creates and compares product formats ids by value")]
  public static void ProductFormatId_CreatedAndEqual_WhenValueIsPositive() {
    var id = new ProductFormatId(1);

    Assert.Equal(1, id.Value);
    Assert.Equal(id, new ProductFormatId(1));
  }
}