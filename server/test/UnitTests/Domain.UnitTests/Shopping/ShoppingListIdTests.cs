using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;

namespace Metaspesa.Domain.UnitTests.Shopping;

public class ShoppingListIdTests {
  [Fact(DisplayName = "Creates and compares shopping list IDs by value")]
  public void Constructor_CreatesValueObject_WhenIdIsPositive() {
    var rawId = Guid.Parse("00000000-0000-7000-8000-000000000001");
    var firstId = new ShoppingListId(rawId);
    var secondId = new ShoppingListId(rawId);

    Assert.Equal(firstId, secondId);
  }

  [Fact(DisplayName = "Rejects invalid shopping list ID")]
  public void Constructor_ThrowsExactException_WhenIdIsInvalid() {
    Assert.Throws<InvalidShoppingListIdException>(() =>
      new ShoppingListId(Guid.Empty));
  }
}