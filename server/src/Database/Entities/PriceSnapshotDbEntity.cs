using Metaspesa.Domain.Markets;
using Metaspesa.Domain.SharedKernel;

namespace Metaspesa.Database.Entities;

internal class PriceSnapshotDbEntity {
  public int Id { get; set; }
  public int ProductFormatId { get; set; }
  public decimal PriceAmount { get; set; }
  public string CurrencyCode { get; set; } = "EUR";
  public DateTime ObservedAt { get; set; }

  public ProductFormatDbEntity ProductFormat { get; set; } = null!;
  public ICollection<PurchaseItemDbEntity> PurchaseItems { get; set; } = [];

  public PriceSnapshot MapToDomain() => new(
    new PriceSnapshotId(Id),
    new ProductFormatId(ProductFormatId),
    new Money(PriceAmount),
    DateTime.SpecifyKind(ObservedAt, DateTimeKind.Utc));
}
