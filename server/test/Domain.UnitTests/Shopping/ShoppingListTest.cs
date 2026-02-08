using Metaspesa.Domain.Shopping;

namespace Metaspesa.Domain.UnitTests.Shopping;

public static class ShoppingListTest {
  public class IsNamed {
    [Fact(DisplayName = "Is named when name is not null or whitespace")]
    public void ShoppingList_IsNamed_WhenNameIsNotNullOrWhitespace() {
      // Arrange
      var list = new ShoppingList("Groceries", []);

      // Act & Assert
      Assert.True(list.IsNamed);
    }

    [Theory(DisplayName = "Is not named when name is null or whitespace")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ShoppingList_IsNotNamed_WhenNameIsNullOrWhitespace(
      string? name
    ) {
      // Arrange
      var list = new ShoppingList(name!, []);

      // Act & Assert
      Assert.False(list.IsNamed);
    }
  }

  public class Intersecting {
    [Fact(DisplayName = "Returns empty shopping list when source list is empty")]
    public void ShoppingList_ReturnsEmptyList_WhenSourceListIsEmpty() {
      // Arrange
      var list = new ShoppingList("Groceries", []);
      IReadOnlyCollection<Product> products = [
        new RegisteredItem("Milk", null, 2.0f),
        new RegisteredItem("Watermelon", null, 10.0f)
      ];

      // Act
      ShoppingList intersectingList = list.Intersecting(products);

      // Assert
      Assert.Empty(intersectingList);
    }

    [Fact(DisplayName = "Returns empty shopping list when given list is empty")]
    public void ShoppingList_ReturnsEmptyList_WhenGivenListIsEmpty() {
      // Arrange
      var list = new ShoppingList("Groceries", [
        new ShoppingItem("Milk", null, 1.5f, false),
        new ShoppingItem("Bread", null, 2.5f, false),
        new ShoppingItem("Eggs", null, 12.5f, false)
      ]);

      IReadOnlyCollection<Product> products = [];

      // Act
      ShoppingList intersectingList = list.Intersecting(products);

      // Assert
      Assert.Empty(intersectingList);
    }

    [Fact(DisplayName = "Returns new shopping list with common items")]
    public void ShoppingList_ReturnsShoppingList_WithCommonItems() {
      // Arrange
      var list = new ShoppingList("Groceries", [
        new ShoppingItem("Milk", null, 1.5f, false),
        new ShoppingItem("Bread", null, 2.5f, false),
        new ShoppingItem("Eggs", null, 12.5f, false)
      ]);

      IReadOnlyCollection<Product> products = [
        new RegisteredItem("Milk", null, 2.0f),
        new RegisteredItem("Watermelon", null, 10.0f)
      ];

      // Act
      ShoppingList intersectingList = list.Intersecting(products);

      // Assert
      Assert.Contains(intersectingList, i => i.NormalizedName == "MILK");
    }

    [Fact(DisplayName = "Removes items that do not intersect with the given products")]
    public void ShoppingList_ReturnsShoppingList_WithoutNonIntersectingItems() {
      // Arrange
      var list = new ShoppingList("Groceries", [
        new ShoppingItem("Milk", null, 1.5f, false),
        new ShoppingItem("Bread", null, 2.5f, false),
        new ShoppingItem("Eggs", null, 12.5f, false)
      ]);

      IReadOnlyCollection<Product> products = [
        new RegisteredItem("Milk", null, 2.0f),
        new RegisteredItem("Watermelon", null, 10.0f)
      ];

      // Act
      ShoppingList intersectingList = list.Intersecting(products);

      // Assert
      Assert.DoesNotContain(intersectingList, i => i.NormalizedName == "BREAD");
      Assert.DoesNotContain(intersectingList, i => i.NormalizedName == "EGGS");
    }

    [Fact(DisplayName = "Returns new shopping list with same name as source list")]
    public void ShoppingList_ReturnsShoppingList_WithSameNameAsSourceList() {
      // Arrange
      var list = new ShoppingList("Groceries", [
        new ShoppingItem("Milk", null, 1.5f, false),
        new ShoppingItem("Bread", null, 2.5f, false),
        new ShoppingItem("Eggs", null, 12.5f, false)
      ]);

      IReadOnlyCollection<Product> products = [
        new RegisteredItem("Milk", null, 2.0f),
        new RegisteredItem("Watermelon", null, 10.0f)
      ];

      // Act
      ShoppingList intersectingList = list.Intersecting(products);

      // Assert
      Assert.Equal(list.Name, intersectingList.Name);
    }
  }

  public class Without {
    [Fact(DisplayName = "Returns empty shopping list when source list is empty")]
    public void ShoppingList_ReturnsEmptyList_WhenSourceListIsEmpty() {
      // Arrange
      var list = new ShoppingList("Groceries", []);
      IReadOnlyCollection<Product> products = [
        new RegisteredItem("Milk", null, 2.0f),
        new RegisteredItem("Watermelon", null, 10.0f)
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
        new ShoppingItem("Milk", null, 1.5f, false),
        new ShoppingItem("Bread", null, 2.5f, false),
        new ShoppingItem("Eggs", null, 12.5f, false)
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
        new ShoppingItem("Milk", null, 1.5f, false),
        new ShoppingItem("Bread", null, 2.5f, false),
        new ShoppingItem("Eggs", null, 12.5f, false)
      ]);

      IReadOnlyCollection<Product> products = [
        new RegisteredItem("Milk", null, 2.0f),
        new RegisteredItem("Watermelon", null, 10.0f)
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
        new ShoppingItem("Milk", null, 1.5f, false),
        new ShoppingItem("Bread", null, 2.5f, false),
        new ShoppingItem("Eggs", null, 12.5f, false)
      ]);

      IReadOnlyCollection<Product> products = [
        new RegisteredItem("Milk", null, 2.0f),
        new RegisteredItem("Watermelon", null, 10.0f)
      ];

      // Act
      ShoppingList withoutList = list.Without(products);

      // Assert
      Assert.Equal(list.Name, withoutList.Name);
    }
  }
}