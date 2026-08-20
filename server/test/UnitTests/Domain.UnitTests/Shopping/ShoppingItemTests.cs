using Metaspesa.Domain.Markets;
using Metaspesa.Domain.SharedKernel;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Domain.UnitTests.Shopping;

public class ShoppingItemTests {
  [Fact(DisplayName = "Creates shopping item from typed values")]
  public void Constructor_ExposesTypedValues() {
    var item = new ShoppingItem(new ProductFormatId(3), new PositiveAmount(2), true);

    Assert.Equal(new ProductFormatId(3), item.ProductFormatId);
    Assert.Equal(new PositiveAmount(2), item.Amount);
    Assert.True(item.IsChecked);
  }
}