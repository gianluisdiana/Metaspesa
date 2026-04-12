using Metaspesa.Domain.Shopping;

namespace Metaspesa.Domain.UnitTests.Shopping;

public static class PricePolicyTest {
  public class IsValidPrice {
    [Theory(DisplayName = "Returns true for non-negative values")]
    [InlineData(0f)]
    [InlineData(0.01f)]
    [InlineData(1f)]
    [InlineData(999.99f)]
    public void PricePolicy_IsValidPrice_ReturnsTrue_ForNonNegativeValues(float price) {
      // Act
      bool isValid = PricePolicy.IsValidPrice(price);

      // Assert
      Assert.True(isValid);
    }

    [Theory(DisplayName = "Returns false for negative values")]
    [InlineData(-0.01f)]
    [InlineData(-1f)]
    [InlineData(-100f)]
    public void PricePolicy_IsValidPrice_ReturnsFalse_ForNegativeValues(float price) {
      // Act
      bool isValid = PricePolicy.IsValidPrice(price);

      // Assert
      Assert.False(isValid);
    }
  }
}
