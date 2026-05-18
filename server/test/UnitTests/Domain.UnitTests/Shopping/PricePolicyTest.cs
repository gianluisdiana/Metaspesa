using Metaspesa.Domain.Shopping;

namespace Metaspesa.Domain.UnitTests.Shopping;

public static class PricePolicyTest {
  public class IsValidPrice {
    [Theory(DisplayName = "Returns true for non-negative values")]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(999.99)]
    public void PricePolicy_IsValidPrice_ReturnsTrue_ForNonNegativeValues(double price) {
      // Act
      bool isValid = PricePolicy.IsValidPrice((decimal)price);

      // Assert
      Assert.True(isValid);
    }

    [Theory(DisplayName = "Returns false for negative values")]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void PricePolicy_IsValidPrice_ReturnsFalse_ForNegativeValues(double price) {
      // Act
      bool isValid = PricePolicy.IsValidPrice((decimal)price);

      // Assert
      Assert.False(isValid);
    }
  }
}