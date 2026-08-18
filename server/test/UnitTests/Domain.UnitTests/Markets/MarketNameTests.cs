using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.UnitTests.Markets;

public static class MarketNameTests {
  [Theory(DisplayName = "Trims valid market names")]
  [InlineData(" Market ", "Market")]
  [InlineData("Market", "Market")]
  public static void MarketName_Normalized_WhenValueIsValid(
    string value, string expected
  ) {
    var name = new MarketName(value);

    Assert.Equal(expected, name.Value);
    Assert.Equal(name, new MarketName(expected));
  }

  [Theory(DisplayName = "Rejects empty market names")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData(" ")]
  public static void MarketName_ThrowsSpecificException_WhenValueIsInvalid(string? value) =>
    Assert.Throws<InvalidMarketNameException>(() => new MarketName(value!));
}
