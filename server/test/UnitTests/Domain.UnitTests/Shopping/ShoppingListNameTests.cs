using Metaspesa.Domain.Shopping;
using Metaspesa.Domain.Shopping.Errors;

namespace Metaspesa.Domain.UnitTests.Shopping;

public class ShoppingListNameTests {
  [Fact(DisplayName = "Trims and compares shopping list names by value")]
  public void Constructor_NormalizesName() {
    Assert.Equal(new ShoppingListName("Weekly"), new ShoppingListName("  Weekly  "));
  }

  [Theory(DisplayName = "Rejects invalid shopping list name")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public void Constructor_ThrowsExactException_WhenNameIsInvalid(string? value) {
    Assert.Throws<InvalidShoppingListNameException>(() => new ShoppingListName(value!));
  }
}