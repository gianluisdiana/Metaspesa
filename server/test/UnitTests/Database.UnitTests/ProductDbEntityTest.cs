using Metaspesa.Database.Entities;
using Metaspesa.Domain.Markets;

namespace Metaspesa.Database.UnitTests;

public class ProductDbEntityTest {
  [Fact(DisplayName = "Maps product name to domain product")]
  public void Entity_MapsToDomain_WithName() {
    // Arrange
    ProductDbEntity entity = ProductWithHistory(name: "Milk");

    // Act
    MarketProduct result = entity.MapToDomain();

    // Assert
    Assert.Equal("Milk", result.Name);
  }

  [Fact(DisplayName = "Maps product brand to domain product")]
  public void Entity_MapsToDomain_WithBrand() {
    // Arrange
    ProductDbEntity entity = ProductWithHistory(brandName: "Hacendado");

    // Act
    MarketProduct result = entity.MapToDomain();

    // Assert
    Assert.Equal("Hacendado", result.Brand.Name);
  }

  [Fact(DisplayName = "Maps only formats from latest history date")]
  public void Entity_MapsToDomain_WithOnlyLatestHistoryDateFormats() {
    // Arrange
    var older = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    var latest = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc);
    ProductDbEntity entity = ProductWithHistory(
      History(1.00m, 1, "L", older),
      History(2.00m, 500, "ml", latest));

    // Act
    MarketProduct result = entity.MapToDomain();

    // Assert
    Assert.Equal(500, result.Formats.Single().Quantity.Value);
  }

  [Fact(DisplayName = "Maps all formats from latest history date")]
  public void Entity_MapsToDomain_WithAllFormatsFromLatestHistoryDate() {
    // Arrange
    var older = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    var latest = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc);
    ProductDbEntity entity = ProductWithHistory(
      History(1.00m, 1, "L", older),
      History(2.00m, 500, "ml", latest),
      History(3.00m, 1, "kg", latest));

    // Act
    MarketProduct result = entity.MapToDomain();

    // Assert
    Assert.Equal(2, result.Formats.Count);
  }

  private static ProductDbEntity ProductWithHistory(
    string name = "Default product",
    string brandName = "Default brand"
  ) => ProductWithHistory(
    [History()],
    name,
    brandName);

  private static ProductDbEntity ProductWithHistory(
    IReadOnlyCollection<ProductsHistoryDbEntity> history,
    string name = "Default product",
    string brandName = "Default brand"
  ) => new() {
    Name = name,
    Brand = new ProductBrandDbEntity { Name = brandName },
    History = [.. history]
  };

  private static ProductDbEntity ProductWithHistory(
    params ProductsHistoryDbEntity[] history
  ) => ProductWithHistory(history, "Default product", "Default brand");

  private static ProductsHistoryDbEntity History(
    decimal price = 1.99m,
    decimal quantity = 1,
    string unitOfMeasure = "L",
    DateTime? createdAt = null,
    string imageUrl = "https://example.com/product.png"
  ) => new() {
    Price = price,
    CreatedAt = createdAt ?? new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
    ProductFormat = new ProductFormatDbEntity {
      Quantity = quantity,
      ImageUrl = imageUrl,
      UnitOfMeasure = new UnitOfMeasureDbEntity { Code = unitOfMeasure }
    }
  };
}
