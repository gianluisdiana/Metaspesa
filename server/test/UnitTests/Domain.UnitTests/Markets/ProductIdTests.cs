using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.UnitTests.Markets;

public static class ProductIdTests {
  [Fact(DisplayName = "Creates and compares product ids by value")]
  public static void ProductId_CreatedAndEqual_WhenValueIsPositive() {
    var id = new ProductId(1);

    Assert.Equal(1, id.Value);
    Assert.Equal(id, new ProductId(1));
  }

  [Theory(DisplayName = "Rejects non-positive product ids")]
  [InlineData(0)]
  [InlineData(-1)]
  public static void ProductId_ThrowsSpecificException_WhenValueIsInvalid(int value) =>
    Assert.Throws<InvalidProductIdException>(() => new ProductId(value));
}