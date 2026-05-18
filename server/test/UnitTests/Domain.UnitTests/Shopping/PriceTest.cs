using Metaspesa.Domain.Shopping;

namespace Metaspesa.Domain.UnitTests.Shopping;

public static class PriceTest {
  public class IsZero {
    [Theory(DisplayName = "Returns true when value is below epsilon")]
    [InlineData(0)]
    [InlineData(0.005)]
    [InlineData(0.009)]
    public void Price_IsZero_WhenValueBelowEpsilon(double value) {
      // Arrange
      var price = new Price((decimal)value);

      // Act & Assert
      Assert.True(price.IsZero());
    }

    [Theory(DisplayName = "Returns false when value is at or above epsilon")]
    [InlineData(0.01)]
    [InlineData(0.5)]
    [InlineData(10)]
    public void Price_IsNotZero_WhenValueAtOrAboveEpsilon(double value) {
      // Arrange
      var price = new Price((decimal)value);

      // Act & Assert
      Assert.False(price.IsZero());
    }
  }

  public class Equality {
    [Fact(DisplayName = "Two prices with the same value are equal")]
    public void Price_Equal_WhenSameValue() {
      // Arrange
      var price1 = new Price(5.0m);
      var price2 = new Price(5.0m);

      // Act & Assert
      Assert.Equal(price1, price2);
    }

    [Theory(DisplayName = "Two prices within epsilon are equal")]
    [InlineData(5.0, 5.005)]
    [InlineData(5.0, 4.995)]
    public void Price_Equal_WhenDifferenceWithinEpsilon(double a, double b) {
      // Arrange
      var price1 = new Price((decimal)a);
      var price2 = new Price((decimal)b);

      // Act & Assert
      Assert.Equal(price1, price2);
    }

    [Fact(DisplayName = "Two prices with different values are not equal")]
    public void Price_NotEqual_WhenDifferentValues() {
      // Arrange
      var price1 = new Price(5.0m);
      var price2 = new Price(10.0m);

      // Act & Assert
      Assert.NotEqual(price1, price2);
    }

    [Fact(DisplayName = "Equal prices have the same hash code")]
    public void Price_SameHashCode_WhenEqual() {
      // Arrange
      var price1 = new Price(5.0m);
      var price2 = new Price(5.0m);

      // Act & Assert
      Assert.Equal(price1.GetHashCode(), price2.GetHashCode());
    }
  }
}