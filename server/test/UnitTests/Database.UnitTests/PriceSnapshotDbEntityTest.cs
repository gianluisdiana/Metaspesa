using Metaspesa.Database.Entities;
using Metaspesa.Domain.Markets;

namespace Metaspesa.Database.UnitTests;

public class PriceSnapshotDbEntityTest {
  [Fact(DisplayName = "Maps format quantity value to domain format")]
  public void Entity_MapsToDomainFormat_WithQuantityValue() {
    // Arrange
    PriceSnapshotDbEntity entity = History(quantity: 500);

    // Act
    ProductFormat result = entity.MapToDomainFormat();

    // Assert
    Assert.Equal(500, result.Quantity.Value);
  }

  [Fact(DisplayName = "Maps format unit of measure to domain format")]
  public void Entity_MapsToDomainFormat_WithUnitOfMeasure() {
    // Arrange
    PriceSnapshotDbEntity entity = History(unitOfMeasure: "ml");

    // Act
    ProductFormat result = entity.MapToDomainFormat();

    // Assert
    Assert.Equal("ml", result.Quantity.UnitOfMeasure);
  }

  [Fact(DisplayName = "Maps history price to domain format")]
  public void Entity_MapsToDomainFormat_WithPrice() {
    // Arrange
    PriceSnapshotDbEntity entity = History(price: 2.49m);

    // Act
    ProductFormat result = entity.MapToDomainFormat();

    // Assert
    Assert.Equal(2.49m, result.Price.Value);
  }

  [Fact(DisplayName = "Maps format image URL to domain format")]
  public void Entity_MapsToDomainFormat_WithImageUrl() {
    // Arrange
    var imageUrl = new Uri("https://example.com/milk.png");
    PriceSnapshotDbEntity entity = History(imageUrl: imageUrl.ToString());

    // Act
    ProductFormat result = entity.MapToDomainFormat();

    // Assert
    Assert.Equal(imageUrl, result.ImageUrl);
  }

  private static PriceSnapshotDbEntity History(
    decimal price = 1.99m,
    decimal quantity = 1,
    string unitOfMeasure = "L",
    string imageUrl = "https://example.com/product.png"
  ) => new() {
    PriceAmount = price,
    ProductFormat = new ProductFormatDbEntity {
      Quantity = quantity,
      ImageUrl = imageUrl,
      UnitOfMeasure = new UnitOfMeasureDbEntity { Code = unitOfMeasure }
    }
  };
}
