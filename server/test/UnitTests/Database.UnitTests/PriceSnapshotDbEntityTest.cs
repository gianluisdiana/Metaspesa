using Metaspesa.Database.Entities;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Markets.Errors;
using Metaspesa.Domain.SharedKernel;

namespace Metaspesa.Database.UnitTests;

public static class PriceSnapshotDbEntityTest {
  [Fact(DisplayName = "Maps persisted price snapshot to immutable domain observation")]
  public static void Entity_MapsToPriceSnapshot() {
    var observedAt = new DateTime(
      2026, 7, 28, 12, 0, 0, DateTimeKind.Unspecified);
    var entity = new PriceSnapshotDbEntity {
      Id = 5,
      ProductFormatId = 7,
      PriceAmount = 1.99m,
      ObservedAt = observedAt,
    };

    PriceSnapshot snapshot = entity.MapToDomain();

    Assert.Equal(new PriceSnapshotId(5), snapshot.Id);
    Assert.Equal(new ProductFormatId(7), snapshot.ProductFormatId);
    Assert.Equal(new Money(1.99m), snapshot.Price);
    Assert.Equal(DateTimeKind.Utc, snapshot.ObservedAt.Kind);
    Assert.Equal(observedAt, snapshot.ObservedAt);
  }

  [Fact(DisplayName = "Throws specific exception for invalid persisted snapshot id")]
  public static void Entity_ThrowsSpecificException_WhenSnapshotIdIsInvalid() {
    var entity = new PriceSnapshotDbEntity {
      Id = 0,
      ProductFormatId = 7,
      PriceAmount = 1.99m,
      ObservedAt = new DateTime(
        2026, 7, 28, 0, 0, 0, DateTimeKind.Unspecified),
    };

    Assert.Throws<InvalidPriceSnapshotIdException>(entity.MapToDomain);
  }
}
