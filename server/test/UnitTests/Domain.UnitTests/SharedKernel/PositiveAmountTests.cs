using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.SharedKernel.Errors;

namespace Metaspesa.Domain.UnitTests.SharedKernel;

public class PositiveAmountTests {
  [Fact(DisplayName = "Creates positive amount")]
  public void Constructor_CreatesAmount_WhenValueIsPositive() {
    var amount = new PositiveAmount(2);

    Assert.Equal(2, amount.Value);
  }

  [Theory(DisplayName = "Rejects non-positive amount")]
  [InlineData(0)]
  [InlineData(-1)]
  public void Constructor_ThrowsExactException_WhenValueIsNotPositive(int value) {
    Assert.Throws<InvalidPositiveAmountException>(() => new PositiveAmount(value));
  }
}