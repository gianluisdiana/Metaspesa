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

  [Fact(DisplayName = "Maps latest price snapshot for each format")]
  public void Entity_MapsToDomain_WithLatestSnapshotForEachFormat() {
    // Arrange
    var older = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    var latest = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc);
    ProductDbEntity entity = ProductWithHistory(
      History(1.00m, 1, "L", older),
      History(2.00m, 500, "ml", latest));

    // Act
    MarketProduct result = entity.MapToDomain();

    // Assert
    Assert.Equal(2, result.Formats.Count);
  }

  [Fact(DisplayName = "Maps every format using its latest price snapshot")]
  public void Entity_MapsToDomain_WithEveryFormatUsingLatestSnapshot() {
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
    Assert.Equal(3, result.Formats.Count);
  }

  private static ProductDbEntity ProductWithHistory(
    string name = "Default product",
    string brandName = "Default brand"
  ) => ProductWithHistory(
    [History()],
    name,
    brandName);

  private static ProductDbEntity ProductWithHistory(
    IReadOnlyCollection<PriceSnapshotDbEntity> history,
    string name = "Default product",
    string brandName = "Default brand"
  ) => new ProductDbEntity {
    Name = name,
    Brand = new ProductBrandDbEntity { Name = brandName },
    Formats = [.. history.Select(h => h.ProductFormat)]
  };

  private static ProductDbEntity ProductWithHistory(
    params PriceSnapshotDbEntity[] history
  ) => ProductWithHistory(history, "Default product", "Default brand");

  private static PriceSnapshotDbEntity History(
    decimal price = 1.99m,
    decimal quantity = 1,
    string unitOfMeasure = "L",
    DateTime? ObservedAt = null,
    string imageUrl = "https://example.com/product.png"
  ) => new PriceSnapshotDbEntity {
    PriceAmount = price,
    ObservedAt = ObservedAt ?? new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
    ProductFormat = new ProductFormatDbEntity {
      Quantity = quantity,
      ImageUrl = imageUrl,
      UnitOfMeasure = new UnitOfMeasureDbEntity { Code = unitOfMeasure },
      PriceSnapshots = []
    }
  }.Tap(snapshot => snapshot.ProductFormat.PriceSnapshots.Add(snapshot));
}

internal static class TestObjectExtensions {
  public static T Tap<T>(this T value, Action<T> action) {
    action(value);
    return value;
  }
}
