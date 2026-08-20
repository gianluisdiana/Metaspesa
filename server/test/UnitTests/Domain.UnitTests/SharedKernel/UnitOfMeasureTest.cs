using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.SharedKernel.Errors;

namespace Metaspesa.Domain.UnitTests.SharedKernel;

public static class UnitOfMeasureTest {
  public class Constructor {
    [Theory(DisplayName = "Creates unit of measure when value is supported")]
    [InlineData("unit", "unit")]
    [InlineData("g", "g")]
    [InlineData("kg", "kg")]
    [InlineData("ml", "ml")]
    [InlineData("l", "l")]
    [InlineData(" KG ", "kg")]
    public void UnitOfMeasure_Created_WhenValueIsSupported(
      string value,
      string expectedValue
    ) {
      // Act
      var unitOfMeasure = new UnitOfMeasure(value);

      // Assert
      Assert.Equal(expectedValue, unitOfMeasure.Value);
    }

    [Theory(DisplayName = "Throws specific exception when value is invalid")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("box")]
    public void UnitOfMeasure_ThrowsInvalidUnitOfMeasureException_WhenValueIsInvalid(string value) {
      // Act
      InvalidUnitOfMeasureException exception = Assert.Throws<InvalidUnitOfMeasureException>(
        () => new UnitOfMeasure(value));

      // Assert
      Assert.Equal(value, exception.Unit);
      Assert.IsAssignableFrom<DomainException>(exception);
    }
  }

  public class Equality {
    [Fact(DisplayName = "Two units representing the same unit are equal")]
    public void UnitOfMeasure_Equal_WhenSameNormalizedValue() {
      // Arrange
      var unit1 = new UnitOfMeasure("kg");
      var unit2 = new UnitOfMeasure(" KG ");

      // Act & Assert
      Assert.Equal(unit1, unit2);
    }
  }
}