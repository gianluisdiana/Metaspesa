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
      var product = new Product(name, null);

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
      var product = new Product(name, null);

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
      var product = new Product(name, null);

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
      var product = new Product(name, null);

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
      var product = new Product(Name, null);

      // Act & Assert
      Assert.Equal(ExpectedNormalizedName, product.NormalizedName);
    }
  }
}