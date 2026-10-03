using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;

namespace Metaspesa.Domain.UnitTests.Shopping;

public class ShoppingItemIdTests {
  [Fact(DisplayName = "Creates and compares shopping item IDs by value")]
  public void Constructor_CreatesValueObject_WhenIdIsValid() {
    var value = Guid.Parse("00000000-0000-7000-8000-000000000001");

    var id = new ShoppingItemId(value);

    Assert.Equal(new ShoppingItemId(value), id);
    Assert.Equal(value, id.Value);
  }

  [Fact(DisplayName = "Rejects empty shopping item ID")]
  public void Constructor_ThrowsExactException_WhenIdIsEmpty() {
    Assert.Throws<InvalidShoppingItemIdException>(() =>
      new ShoppingItemId(Guid.Empty));
  }

  [Fact(DisplayName = "Formats shopping item ID as its primitive value")]
  public void ToString_ReturnsGuidText() {
    var value = Guid.Parse("00000000-0000-7000-8000-000000000001");

    var id = new ShoppingItemId(value);

    Assert.Equal(value.ToString(), id.ToString());
  }
}