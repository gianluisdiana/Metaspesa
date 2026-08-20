using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.SharedKernel.Errors;

namespace Metaspesa.Domain.UnitTests.SharedKernel;

public static class MoneyTest {
  public class Constructor {
    [Theory(DisplayName = "Creates money when amount is non-negative")]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(12.34)]
    public void Money_Created_WhenAmountIsNonNegative(decimal amount) {
      // Act
      var money = new Money(amount);

      // Assert
      Assert.Equal(amount, money.Amount);
    }

    [Fact(DisplayName = "Throws specific exception when amount is negative")]
    public void Money_ThrowsInvalidMoneyAmountException_WhenAmountIsNegative() {
      // Act
      InvalidMoneyAmountException exception = Assert.Throws<InvalidMoneyAmountException>(
        () => new Money(-0.01m));

      // Assert
      Assert.Equal(-0.01m, exception.Amount);
    }

    [Theory(DisplayName = "Rounds amount to two decimal places")]
    [InlineData(12.345, 12.35)]
    [InlineData(12.344, 12.34)]
    public void Money_RoundsAmountToTwoDecimalPlaces(
      decimal amount, decimal expectedRoundedAmount
    ) {
      // Act
      var money = new Money(amount);

      // Assert
      Assert.Equal(expectedRoundedAmount, money.Amount);
    }
  }

  public class Equality {
    [Fact(DisplayName = "Two money values with the same amount are equal")]
    public void Money_Equal_WhenSameAmount() {
      // Arrange
      var money1 = new Money(5.0m);
      var money2 = new Money(5.0m);

      // Act & Assert
      Assert.Equal(money1, money2);
    }
  }
}