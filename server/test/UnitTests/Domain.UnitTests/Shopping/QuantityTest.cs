using Metaspesa.Domain.Shopping;

namespace Metaspesa.Domain.UnitTests.Shopping;

public static class QuantityTest {
  public class Constructor {
    [Fact(DisplayName = "Creates quantity when value is within maximum length")]
    public void Quantity_Created_WhenValueIsWithinMaximumLength() {
      // Arrange
      string value = new('a', Quantity.MaximumLength);

      // Act
      var quantity = new Quantity(value);

      // Assert
      Assert.Equal(value, quantity.Value);
    }
  }

  public class FromNullable {
    [Fact(DisplayName = "Returns null when value is null")]
    public void Quantity_ReturnsNull_WhenValueIsNull() {
      // Act
      var quantity = Quantity.FromNullable(null);

      // Assert
      Assert.Null(quantity);
    }

    [Fact(DisplayName = "Returns quantity when value is not null")]
    public void Quantity_ReturnsQuantity_WhenValueIsNotNull() {
      // Act
      var quantity = Quantity.FromNullable("1 kg");

      // Assert
      Assert.NotNull(quantity);
      Assert.Equal("1 kg", quantity.Value);
    }
  }
}