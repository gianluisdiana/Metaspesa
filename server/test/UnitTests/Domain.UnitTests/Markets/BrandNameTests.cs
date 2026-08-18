using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.UnitTests.Markets;

public static class BrandNameTests {
  [Theory(DisplayName = "Rejects empty brand names")]
  [InlineData("")]
  [InlineData(" ")]
  public static void BrandName_ThrowsSpecificException_WhenValueIsInvalid(string value) =>
    Assert.Throws<InvalidBrandNameException>(() => new BrandName(value));
}
