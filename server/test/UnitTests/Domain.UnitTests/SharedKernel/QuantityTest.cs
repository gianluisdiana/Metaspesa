using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.SharedKernel.Errors;

namespace Metaspesa.Domain.UnitTests.SharedKernel;

public static class QuantityTest {
  public class Constructor {
    [Fact(DisplayName = "Creates quantity when amount is positive and unit is valid")]
    public void Quantity_Created_WhenAmountIsPositiveAndUnitIsValid() {
      // Arrange
      var unitOfMeasure = new UnitOfMeasure("kg");

      // Act
      var quantity = new Quantity(2.5m, unitOfMeasure);

      // Assert
      Assert.Equal(2.5m, quantity.Amount);
      Assert.Equal(unitOfMeasure, quantity.UnitOfMeasure);
    }

    [Theory(DisplayName = "Throws specific exception when amount is not positive")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Quantity_ThrowsInvalidQuantityAmountException_WhenAmountIsNotPositive(double amount) {
      // Arrange
      var unitOfMeasure = new UnitOfMeasure("unit");

      // Act
      InvalidQuantityAmountException exception = Assert.Throws<InvalidQuantityAmountException>(
        () => new Quantity((decimal)amount, unitOfMeasure));

      // Assert
      Assert.Equal((decimal)amount, exception.Amount);
      Assert.IsAssignableFrom<DomainException>(exception);
    }
  }

  public class Equality {
    [Fact(DisplayName = "Two quantities with the same amount and unit are equal")]
    public void Quantity_Equal_WhenSameAmountAndUnit() {
      // Arrange
      var quantity1 = new Quantity(1.5m, new UnitOfMeasure("kg"));
      var quantity2 = new Quantity(1.5m, new UnitOfMeasure("kg"));

      // Act & Assert
      Assert.Equal(quantity1, quantity2);
    }
  }
}