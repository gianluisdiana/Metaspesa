using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.UnitTests.Markets;

public static class MarketIdTests {
  [Fact(DisplayName = "Creates and compares market ids by value")]
  public static void MarketId_CreatedAndEqual_WhenValueIsPositive() {
    var id = new MarketId(1);

    Assert.Equal(1, id.Value);
    Assert.Equal(id, new MarketId(1));
  }

  [Theory(DisplayName = "Rejects non-positive market ids")]
  [InlineData(0)]
  [InlineData(-1)]
  public static void MarketId_ThrowsSpecificException_WhenValueIsInvalid(int value) =>
    Assert.Throws<InvalidMarketIdException>(() => new MarketId(value));
}