using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Domain.UnitTests.Markets;

public static class ProductNameTests {
  [Theory(DisplayName = "Rejects empty product names")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData(" ")]
  public static void ProductName_ThrowsSpecificException_WhenValueIsInvalid(string? value) =>
    Assert.Throws<InvalidProductNameException>(() => new ProductName(value!));

  [Fact(DisplayName = "Trims product and brand names")]
  public static void ProductAndBrandNames_Normalized_WhenValuesAreValid() {
    Assert.Equal("Milk", new ProductName(" Milk ").Value);
    Assert.Equal("Acme", new BrandName(" Acme ").Value);
  }
}