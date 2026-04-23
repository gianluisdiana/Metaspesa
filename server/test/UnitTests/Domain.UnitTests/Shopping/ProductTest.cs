using Metaspesa.Domain.Shopping;

namespace Metaspesa.Domain.UnitTests.Shopping;

public static class ProductTest {
  public class NormalizedName {
    [Theory(DisplayName = "Trims the product name")]
    [InlineData("  APPLE")]
    [InlineData("APPLE  ")]
    [InlineData("  APPLE  ")]
    public void Product_NormalizesName_RemovingWhitespaces(
      string name
    ) {
      // Arrange
      var product = new Product(name, null, Price.Empty);

      // Act & Assert
      Assert.Equal(name.Trim(), product.NormalizedName);
    }

    [Theory(DisplayName = "Converts the product name to uppercase")]
    [InlineData("peach")]
    [InlineData("PEACH")]
    [InlineData("PeAcH")]
    public void Product_NormalizesName_ConvertingToUppercase(
      string name
    ) {
      // Arrange
      var product = new Product(name, null, Price.Empty);

      // Act & Assert
      Assert.Equal(name.ToUpperInvariant(), product.NormalizedName);
    }

    [Theory(DisplayName = "Replaces spaces with dashes in the product name")]
    [InlineData("GREEN APPLE")]
    [InlineData("REALLY GREEN APPLE")]
    [InlineData("REALLY REALLY GREEN APPLE")]
    public void Product_NormalizesName_ReplacingSpacesWithDashes(
      string name
    ) {
      // Arrange
      var product = new Product(name, null, Price.Empty);

      // Act & Assert
      Assert.Equal(name.Replace(' ', '-'), product.NormalizedName);
    }

    [Theory(
      DisplayName = "Collapses multiple spaces into a single dash in the product name")]
    [InlineData("GREEN   APPLE")]
    [InlineData("REALLY   GREEN    APPLE")]
    public void Product_NormalizesName_CollapsingMultipleSpaces(
      string name
    ) {
      // Arrange
      var product = new Product(name, null, Price.Empty);

      // Act & Assert
      Assert.DoesNotContain(
        "--",
        product.NormalizedName,
        StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact(DisplayName = "Normalizes the product name correctly")]
    public void Product_NormalizesName_Correctly() {
      // Arrange
      const string Name = "  sweet  potato  ";
      const string ExpectedNormalizedName = "SWEET-POTATO";
      var product = new Product(Name, null, Price.Empty);

      // Act & Assert
      Assert.Equal(ExpectedNormalizedName, product.NormalizedName);
    }
  }

  public class Equality {
    [Fact(DisplayName = "Products with the same normalized name and quantity are equal")]
    public void Products_AreEquals_WhenSameNormalizedNameAndQuantity() {
      // Arrange
      var product1 = new Product("  Apple  ", "1 kg", Price.Empty);
      var product2 = new Product("APPLE", "1 kg", Price.Empty);

      // Act & Assert
      Assert.Equal(product1, product2);
    }

    [Fact(DisplayName = "Products with different normalized names are not equal")]
    public void Products_AreNotEquals_WhenDifferentNormalizedNames() {
      // Arrange
      var product1 = new Product("Apple", "1 kg", Price.Empty);
      var product2 = new Product("Banana", "1 kg", Price.Empty);

      // Act & Assert
      Assert.NotEqual(product1, product2);
    }

    [Fact(DisplayName = "Products with different quantities are not equal")]
    public void Products_AreNotEquals_WhenDifferentQuantities() {
      // Arrange
      var product1 = new Product("Apple", "1 kg", Price.Empty);
      var product2 = new Product("Apple", "500 g", Price.Empty);

      // Act & Assert
      Assert.NotEqual(product1, product2);
    }

    [Fact(DisplayName = "Products with different prices are equal")]
    public void Products_AreEquals_WhenDifferentPrices() {
      // Arrange
      var product1 = new Product("Apple", "1 kg", new Price(1));
      var product2 = new Product("Apple", "1 kg", new Price(2));

      // Act & Assert
      Assert.Equal(product1, product2);
    }

    [Fact(DisplayName = "Hash codes remain consistent for equal products")]
    public void Products_HaveSameHashCode_WhenEqual() {
      // Arrange
      var product1 = new Product("  Apple  ", "1 kg", Price.Empty);
      var product2 = new Product("APPLE", "1 kg", Price.Empty);

      // Act & Assert
      Assert.Equal(product1.GetHashCode(), product2.GetHashCode());
    }
  }

  public class HasSamePrice {
    [Fact(DisplayName = "Returns true when both products have the same price")]
    public void Products_HasSamePrice_WhenSamePrice() {
      // Arrange
      var product1 = new Product("Apple", "1 kg", new Price(2.5f));
      var product2 = new Product("Apple", "1 kg", new Price(2.5f));

      // Act
      bool hasSamePrice = product1.HasSamePrice(product2);

      // Assert
      Assert.True(hasSamePrice);
    }

    [Fact(DisplayName = "Returns false when products have different prices")]
    public void Products_HasSamePrice_ReturnsFalse_WhenDifferentPrices() {
      // Arrange
      var product1 = new Product("Apple", "1 kg", new Price(2.5f));
      var product2 = new Product("Apple", "1 kg", new Price(5.0f));

      // Act
      bool hasSamePrice = product1.HasSamePrice(product2);

      // Assert
      Assert.False(hasSamePrice);
    }

    [Fact(DisplayName = "Returns false when the other product is null")]
    public void Products_HasSamePrice_ReturnsFalse_WhenOtherIsNull() {
      // Arrange
      var product = new Product("Apple", "1 kg", new Price(2.5f));

      // Act
      bool hasSamePrice = product.HasSamePrice(null);

      // Assert
      Assert.False(hasSamePrice);
    }

    [Fact(DisplayName = "Returns true when both prices are within epsilon")]
    public void Products_HasSamePrice_ReturnsTrue_WhenPricesWithinEpsilon() {
      // Arrange
      var product1 = new Product("Apple", "1 kg", new Price(2.0f));
      var product2 = new Product("Apple", "1 kg", new Price(2.005f));

      // Act
      bool hasSamePrice = product1.HasSamePrice(product2);

      // Assert
      Assert.True(hasSamePrice);
    }
  }
}
