using Metaspesa.Domain.Shopping;

namespace Metaspesa.Domain.UnitTests.Shopping;

public static class ShoppingListTest {
  public class IsTemporary {
    [Fact(DisplayName = "Is temporary when name is null or whitespace")]
    public void ShoppingList_IsTemporary_WhenNameIsNullOrWhitespace() {
      // Arrange
      var list = new ShoppingList(null, []);

      // Act
      bool isTemporary = list.IsTemporary();

      // Assert
      Assert.True(isTemporary);
    }

    [Theory(DisplayName = "Is temporary when name is empty or whitespace")]
    [InlineData("")]
    [InlineData("   ")]
    public void ShoppingList_IsTemporary_WhenNameIsEmptyOrWhitespace(string? name) {
      // Arrange
      var list = new ShoppingList(name, []);

      // Act
      bool isTemporary = list.IsTemporary();

      // Assert
      Assert.True(isTemporary);
    }

    [Fact(DisplayName = "Is not temporary when name is not null or whitespace")]
    public void ShoppingList_IsNotTemporary_WhenNameIsNotNullOrWhitespace() {
      // Arrange
      var list = new ShoppingList("Groceries", []);

      // Act
      bool isTemporary = list.IsTemporary();

      // Assert
      Assert.False(isTemporary);
    }
  }

  public class Intersecting {
    [Fact(DisplayName = "Returns empty shopping list when source list is empty")]
    public void ShoppingList_ReturnsEmptyList_WhenSourceListIsEmpty() {
      // Arrange
      var list = new ShoppingList("Groceries", []);
      IReadOnlyCollection<Product> products = [
        new Product("Milk", null, new Price(2.0f)),
        new Product("Watermelon", null, new Price(10.0f))
      ];

      // Act
      ShoppingList intersectingList = list.OnlyWithPriceChangedItems(products);

      // Assert
      Assert.Empty(intersectingList);
    }

    [Fact(DisplayName = "Returns empty shopping list when given list is empty")]
    public void ShoppingList_ReturnsEmptyList_WhenGivenListIsEmpty() {
      // Arrange
      var list = new ShoppingList("Groceries", [
        new ShoppingItem("Milk", null, new Price(1.5f), false),
        new ShoppingItem("Bread", null, new Price(2.5f), false),
        new ShoppingItem("Eggs", null, new Price(12.5f), false)
      ]);

      IReadOnlyCollection<Product> products = [];

      // Act
      ShoppingList intersectingList = list.OnlyWithPriceChangedItems(products);

      // Assert
      Assert.Empty(intersectingList);
    }

    [Fact(DisplayName = "Returns new shopping list with common items")]
    public void ShoppingList_ReturnsShoppingList_WithCommonItems() {
      // Arrange
      var list = new ShoppingList("Groceries", [
        new ShoppingItem("Milk", null, new Price(1.5f), false),
        new ShoppingItem("Bread", null, new Price(2.5f), false),
        new ShoppingItem("Eggs", null, new Price(12.5f), false)
      ]);

      IReadOnlyCollection<Product> products = [
        new Product("Milk", null, new Price(2.0f)),
        new Product("Watermelon", null, new Price(10.0f))
      ];

      // Act
      ShoppingList intersectingList = list.OnlyWithPriceChangedItems(products);

      // Assert
      Assert.Contains(intersectingList, i => i.NormalizedName == "MILK");
    }

    [Fact(DisplayName = "Removes items that do not intersect with the given products")]
    public void ShoppingList_ReturnsShoppingList_WithoutNonIntersectingItems() {
      // Arrange
      var list = new ShoppingList("Groceries", [
        new ShoppingItem("Milk", null, new Price(1.5f), false),
        new ShoppingItem("Bread", null, new Price(2.5f), false),
        new ShoppingItem("Eggs", null, new Price(12.5f), false)
      ]);

      IReadOnlyCollection<Product> products = [
        new Product("Milk", null, new Price(2.0f)),
        new Product("Watermelon", null, new Price(10.0f))
      ];

      // Act
      ShoppingList intersectingList = list.OnlyWithPriceChangedItems(products);

      // Assert
      Assert.DoesNotContain(intersectingList, i => i.NormalizedName == "BREAD");
      Assert.DoesNotContain(intersectingList, i => i.NormalizedName == "EGGS");
    }

    [Fact(DisplayName = "Returns new shopping list with same name as source list")]
    public void ShoppingList_ReturnsShoppingList_WithSameNameAsSourceList() {
      // Arrange
      var list = new ShoppingList("Groceries", [
        new ShoppingItem("Milk", null, new Price(1.5f), false),
        new ShoppingItem("Bread", null, new Price(2.5f), false),
        new ShoppingItem("Eggs", null, new Price(12.5f), false)
      ]);

      IReadOnlyCollection<Product> products = [
        new Product("Milk", null, new Price(2.0f)),
        new Product("Watermelon", null, new Price(10.0f))
      ];

      // Act
      ShoppingList intersectingList = list.OnlyWithPriceChangedItems(products);

      // Assert
      Assert.Equal(list.Name, intersectingList.Name);
    }

    [Fact(DisplayName = "Excludes items whose price matches the registered product")]
    public void ShoppingList_ExcludesItems_WhenPriceMatchesRegisteredProduct() {
      // Arrange
      var list = new ShoppingList("Groceries", [
        new ShoppingItem("Milk", null, new Price(2.0f), false),
      ]);

      IReadOnlyCollection<Product> products = [
        new Product("Milk", null, new Price(2.0f)),
      ];

      // Act
      ShoppingList intersectingList = list.OnlyWithPriceChangedItems(products);

      // Assert
      Assert.Empty(intersectingList);
    }
  }

  public class Without {
    [Fact(DisplayName = "Returns empty shopping list when source list is empty")]
    public void ShoppingList_ReturnsEmptyList_WhenSourceListIsEmpty() {
      // Arrange
      var list = new ShoppingList("Groceries", []);
      IReadOnlyCollection<Product> products = [
        new Product("Milk", null, new Price(2.0f)),
        new Product("Watermelon", null, new Price(10.0f))
      ];

      // Act
      ShoppingList withoutList = list.Without(products);

      // Assert
      Assert.Empty(withoutList);
    }

    [Fact(DisplayName = "Returns same shopping list when given list is empty")]
    public void ShoppingList_ReturnsSameList_WhenGivenListIsEmpty() {
      // Arrange
      var list = new ShoppingList("Groceries", [
        new ShoppingItem("Milk", null, new Price(1.5f), false),
        new ShoppingItem("Bread", null, new Price(2.5f), false),
        new ShoppingItem("Eggs", null, new Price(12.5f), false)
      ]);

      IReadOnlyCollection<Product> products = [];

      // Act
      ShoppingList withoutList = list.Without(products);

      // Assert
      Assert.Equal(list.Items.Count, withoutList.Items.Count);
      Assert.All(list.Items, item =>
        Assert.Contains(withoutList, i => i.NormalizedName == item.NormalizedName));
    }

    [Fact(DisplayName = "Returns new shopping list without items that intersect with the given products")]
    public void ShoppingList_ReturnsShoppingList_WithoutIntersectingItems() {
      // Arrange
      var list = new ShoppingList("Groceries", [
        new ShoppingItem("Milk", null, new Price(1.5f), false),
        new ShoppingItem("Bread", null, new Price(2.5f), false),
        new ShoppingItem("Eggs", null, new Price(12.5f), false)
      ]);

      IReadOnlyCollection<Product> products = [
        new Product("Milk", null, new Price(2.0f)),
        new Product("Watermelon", null, new Price(10.0f))
      ];

      // Act
      ShoppingList withoutList = list.Without(products);

      // Assert
      Assert.DoesNotContain(withoutList, i => i.NormalizedName == "MILK");
    }

    [Fact(DisplayName = "Returns new shopping list with same name as source list")]
    public void ShoppingList_ReturnsShoppingList_WithSameNameAsSourceList() {
      // Arrange
      var list = new ShoppingList("Groceries", [
        new ShoppingItem("Milk", null, new Price(1.5f), false),
        new ShoppingItem("Bread", null, new Price(2.5f), false),
        new ShoppingItem("Eggs", null, new Price(12.5f), false)
      ]);

      IReadOnlyCollection<Product> products = [
        new Product("Milk", null, new Price(2.0f)),
        new Product("Watermelon", null, new Price(10.0f))
      ];

      // Act
      ShoppingList withoutList = list.Without(products);

      // Assert
      Assert.Equal(list.Name, withoutList.Name);
    }
  }

  public class HasCheckedItems {
    [Fact(DisplayName = "Doesn't have checked items when list is empty")]
    public void ShoppingList_DoesNotHaveCheckedItems_WhenListIsEmpty() {
      // Arrange
      var list = new ShoppingList("Groceries", []);

      // Act & Assert
      Assert.False(list.HasCheckedItems());
    }

    [Fact(DisplayName = "Doesn't have checked items when all items are unchecked")]
    public void ShoppingList_DoesNotHaveCheckedItems_WhenAllItemsUnchecked() {
      // Arrange
      var list = new ShoppingList("Groceries", [
        new ShoppingItem("Milk", null, new Price(1.5f), false),
        new ShoppingItem("Bread", null, new Price(2.5f), false),
      ]);

      // Act & Assert
      Assert.False(list.HasCheckedItems());
    }

    [Fact(DisplayName = "Has checked items when at least one item is checked")]
    public void ShoppingList_HasCheckedItems_WhenAtLeastOneItemChecked() {
      // Arrange
      var list = new ShoppingList("Groceries", [
        new ShoppingItem("Milk", null, new Price(1.5f), true),
        new ShoppingItem("Bread", null, new Price(2.5f), false),
      ]);

      // Act & Assert
      Assert.True(list.HasCheckedItems());
    }

    [Fact(DisplayName = "Has checked items when all items are checked")]
    public void ShoppingList_HasCheckedItems_WhenAllItemsChecked() {
      // Arrange
      var list = new ShoppingList("Groceries", [
        new ShoppingItem("Milk", null, new Price(1.5f), true),
        new ShoppingItem("Bread", null, new Price(2.5f), true),
      ]);

      // Act & Assert
      Assert.True(list.HasCheckedItems());
    }
  }

  public class OnlyWithCheckedItems {
    [Fact(DisplayName = "Returns empty list when source list is empty")]
    public void ShoppingList_OnlyWithCheckedItems_ReturnsEmpty_WhenSourceEmpty() {
      // Arrange
      var list = new ShoppingList("Groceries", []);

      // Act
      ShoppingList checkedList = list.OnlyWithCheckedItems();

      // Assert
      Assert.Empty(checkedList);
    }

    [Fact(DisplayName = "Returns empty list when no items are checked")]
    public void ShoppingList_OnlyWithCheckedItems_ReturnsEmpty_WhenNoItemsChecked() {
      // Arrange
      var list = new ShoppingList("Groceries", [
        new ShoppingItem("Milk", null, new Price(1.5f), false),
        new ShoppingItem("Bread", null, new Price(2.5f), false),
      ]);

      // Act
      ShoppingList checkedList = list.OnlyWithCheckedItems();

      // Assert
      Assert.Empty(checkedList);
    }

    [Fact(DisplayName = "Returns only checked items")]
    public void ShoppingList_OnlyWithCheckedItems_ReturnsOnlyCheckedItems() {
      // Arrange
      var checkedItem = new ShoppingItem("Milk", null, new Price(1.5f), true);
      var list = new ShoppingList("Groceries", [
        checkedItem,
        new ShoppingItem("Bread", null, new Price(2.5f), false),
      ]);

      // Act
      ShoppingList checkedList = list.OnlyWithCheckedItems();

      // Assert
      Assert.Single(checkedList);
      Assert.Contains(checkedList, i => i.NormalizedName == checkedItem.NormalizedName);
    }

    [Fact(DisplayName = "Preserves the list name")]
    public void ShoppingList_OnlyWithCheckedItems_PreservesName() {
      // Arrange
      var list = new ShoppingList("Groceries", [
        new ShoppingItem("Milk", null, new Price(1.5f), true),
      ]);

      // Act
      ShoppingList checkedList = list.OnlyWithCheckedItems();

      // Assert
      Assert.Equal(list.Name, checkedList.Name);
    }
  }
}
