using Metaspesa.Domain.Markets;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Domain.UnitTests.Shopping;

public class ShoppingItemTests {
  [Fact(DisplayName = "Creates shopping item from typed values")]
  public void Constructor_ExposesTypedValues() {
    var formatId = new ProductFormatId(Guid.Parse("00000000-0000-7000-8000-000000000003"));
    var amount = new PositiveAmount(2);

    var item = new ShoppingItem(formatId, amount, true);

    Assert.Equal(formatId, item.ProductFormatId);
    Assert.Equal(amount, item.Amount);
    Assert.True(item.IsChecked);
  }
}