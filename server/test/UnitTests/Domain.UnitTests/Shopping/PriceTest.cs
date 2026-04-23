using Metaspesa.Domain.Shopping;

namespace Metaspesa.Domain.UnitTests.Shopping;

public static class PriceTest {
  public class IsZero {
    [Theory(DisplayName = "Returns true when value is below epsilon")]
    [InlineData(0f)]
    [InlineData(0.005f)]
    [InlineData(0.009f)]
    public void Price_IsZero_WhenValueBelowEpsilon(float value) {
      // Arrange
      var price = new Price(value);

      // Act & Assert
      Assert.True(price.IsZero());
    }

    [Theory(DisplayName = "Returns false when value is at or above epsilon")]
    [InlineData(0.01f)]
    [InlineData(0.5f)]
    [InlineData(10f)]
    public void Price_IsNotZero_WhenValueAtOrAboveEpsilon(float value) {
      // Arrange
      var price = new Price(value);

      // Act & Assert
      Assert.False(price.IsZero());
    }
  }

  public class Equality {
    [Fact(DisplayName = "Two prices with the same value are equal")]
    public void Price_Equal_WhenSameValue() {
      // Arrange
      var price1 = new Price(5.0f);
      var price2 = new Price(5.0f);

      // Act & Assert
      Assert.Equal(price1, price2);
    }

    [Theory(DisplayName = "Two prices within epsilon are equal")]
    [InlineData(5.0f, 5.005f)]
    [InlineData(5.0f, 4.995f)]
    public void Price_Equal_WhenDifferenceWithinEpsilon(float a, float b) {
      // Arrange
      var price1 = new Price(a);
      var price2 = new Price(b);

      // Act & Assert
      Assert.Equal(price1, price2);
    }

    [Fact(DisplayName = "Two prices with different values are not equal")]
    public void Price_NotEqual_WhenDifferentValues() {
      // Arrange
      var price1 = new Price(5.0f);
      var price2 = new Price(10.0f);

      // Act & Assert
      Assert.NotEqual(price1, price2);
    }

    [Fact(DisplayName = "Equal prices have the same hash code")]
    public void Price_SameHashCode_WhenEqual() {
      // Arrange
      var price1 = new Price(5.0f);
      var price2 = new Price(5.0f);

      // Act & Assert
      Assert.Equal(price1.GetHashCode(), price2.GetHashCode());
    }
  }
}
