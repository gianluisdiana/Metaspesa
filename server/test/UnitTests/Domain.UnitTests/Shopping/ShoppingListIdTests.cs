using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;

namespace Metaspesa.Domain.UnitTests.Shopping;

public class ShoppingListIdTests {
  [Fact(DisplayName = "Creates and compares shopping list IDs by value")]
  public void Constructor_CreatesValueObject_WhenIdIsPositive() {
    Assert.Equal(new ShoppingListId(1), new ShoppingListId(1));
  }

  [Theory(DisplayName = "Rejects invalid shopping list ID")]
  [InlineData(0)]
  [InlineData(-1)]
  public void Constructor_ThrowsExactException_WhenIdIsInvalid(int value) {
    Assert.Throws<InvalidShoppingListIdException>(() => new ShoppingListId(value));
  }
}
