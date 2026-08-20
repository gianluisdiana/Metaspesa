using Metaspesa.Database.Entities;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Markets.Errors;

namespace Metaspesa.Database.UnitTests;

public static class ProductDbEntityTest {
  [Fact(DisplayName = "Maps product and formats without price snapshots")]
  public static void Entity_MapsToProductAggregate_WithoutPriceSnapshots() {
    ProductDbEntity entity = CreateEntity();
    entity.Formats.Single().PriceSnapshots.Add(new PriceSnapshotDbEntity {
      Id = 8,
      ProductFormatId = 7,
      PriceAmount = 1.99m,
      ObservedAt = new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc),
    });

    Product product = entity.MapToDomain();

    Assert.Equal(new ProductId(3), product.Id);
    Assert.Equal(new ProductName("Milk"), product.Name);
    Assert.Equal(new BrandName("Brand"), product.Brand);
    Assert.Equal(new MarketId(2), product.MarketId);
    ProductFormat format = Assert.Single(product.Formats);
    Assert.Equal(new ProductFormatId(7), format.Id);
    Assert.Equal(1m, format.Quantity.Amount);
    Assert.DoesNotContain(
      typeof(ProductFormat).GetProperties(),
      property => property.PropertyType == typeof(PriceSnapshot));
  }

  [Fact(DisplayName = "Throws specific exception for invalid persisted product id")]
  public static void Entity_ThrowsSpecificException_WhenProductIdIsInvalid() {
    ProductDbEntity entity = CreateEntity();
    entity.Id = 0;

    Assert.Throws<InvalidProductIdException>(entity.MapToDomain);
  }

  private static ProductDbEntity CreateEntity() => new() {
    Id = 3,
    Name = "Milk",
    SuperMarketId = 2,
    Brand = new ProductBrandDbEntity { Name = "Brand" },
    Formats = [
      new ProductFormatDbEntity {
        Id = 7,
        Quantity = 1m,
        ImageUrl = "https://example.com/milk.png",
        UnitOfMeasure = new UnitOfMeasureDbEntity { Code = "l" },
      },
    ],
  };
}