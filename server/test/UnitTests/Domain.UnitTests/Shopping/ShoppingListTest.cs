using Metaspesa.Domain.Shopping;

namespace Metaspesa.Domain.UnitTests.Shopping;

public static class ShoppingListTest {
  public class HasCheckedItems {
    [Fact(DisplayName = "Doesn't have checked items when list is empty")]
    public void ShoppingList_DoesNotHaveCheckedItems_WhenListIsEmpty() {
      // Arrange
      var list = new AShoppingList("Groceries", []);

      // Act & Assert
      Assert.False(list.HasCheckedItems());
    }

    [Fact(DisplayName = "Doesn't have checked items when all items are unchecked")]
    public void ShoppingList_DoesNotHaveCheckedItems_WhenAllItemsUnchecked() {
      // Arrange
      var list = new AShoppingList("Groceries", [
        new AShoppingItem(1, 1, false),
        new AShoppingItem(2, 1, false),
      ]);

      // Act & Assert
      Assert.False(list.HasCheckedItems());
    }

    [Fact(DisplayName = "Has checked items when at least one item is checked")]
    public void ShoppingList_HasCheckedItems_WhenAtLeastOneItemChecked() {
      // Arrange
      var list = new AShoppingList("Groceries", [
        new AShoppingItem(1, 1, true),
        new AShoppingItem(2, 1, false),
      ]);

      // Act & Assert
      Assert.True(list.HasCheckedItems());
    }

    [Fact(DisplayName = "Has checked items when all items are checked")]
    public void ShoppingList_HasCheckedItems_WhenAllItemsChecked() {
      // Arrange
      var list = new AShoppingList("Groceries", [
        new AShoppingItem(1, 1, true),
        new AShoppingItem(2, 1, true),
      ]);

      // Act & Assert
      Assert.True(list.HasCheckedItems());
    }
  }

  public class OnlyWithCheckedItems {
    [Fact(DisplayName = "Returns empty list when source list is empty")]
    public void ShoppingList_OnlyWithCheckedItems_ReturnsEmpty_WhenSourceEmpty() {
      // Arrange
      var list = new AShoppingList("Groceries", []);

      // Act
      AShoppingList checkedList = list.OnlyWithCheckedItems();

      // Assert
      Assert.Empty(checkedList.Items);
    }

    [Fact(DisplayName = "Returns empty list when no items are checked")]
    public void ShoppingList_OnlyWithCheckedItems_ReturnsEmpty_WhenNoItemsChecked() {
      // Arrange
      var list = new AShoppingList("Groceries", [
        new AShoppingItem(1, 1, false),
        new AShoppingItem(2, 1, false),
      ]);

      // Act
      AShoppingList checkedList = list.OnlyWithCheckedItems();

      // Assert
      Assert.Empty(checkedList.Items);
    }

    [Fact(DisplayName = "Returns only checked items")]
    public void ShoppingList_OnlyWithCheckedItems_ReturnsOnlyCheckedItems() {
      // Arrange
      var checkedItem = new AShoppingItem(1, 1, true);
      var list = new AShoppingList("Groceries", [
        checkedItem,
        new AShoppingItem(2, 1, false),
      ]);

      // Act
      AShoppingList checkedList = list.OnlyWithCheckedItems();

      // Assert
      Assert.Single(checkedList.Items);
      Assert.Contains(checkedList.Items, i => i.ReferenceUid == checkedItem.ReferenceUid);
    }

    [Fact(DisplayName = "Preserves the list name")]
    public void ShoppingList_OnlyWithCheckedItems_PreservesName() {
      // Arrange
      var list = new AShoppingList("Groceries", [
        new AShoppingItem(1, 1, true),
      ]);

      // Act
      AShoppingList checkedList = list.OnlyWithCheckedItems();

      // Assert
      Assert.Equal(list.Name, checkedList.Name);
    }
  }
}
