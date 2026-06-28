using System.Diagnostics;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Database.Entities;

internal class PriceSnapshotDbEntity {
  public int Id { get; set; }
  public int ProductFormatId { get; set; }
  public decimal PriceAmount { get; set; }
  public string CurrencyCode { get; set; } = "EUR";
  public DateTime ObservedAt { get; set; }

  public ProductFormatDbEntity ProductFormat { get; set; } = null!;
  public ICollection<PurchaseItemDbEntity> PurchaseItems { get; set; } = [];

  public ProductFormat MapToDomainFormat() {
    Debug.Assert(ProductFormat is not null);

    return new(
      Quantity: new AQuantity(
        value: (float)ProductFormat.Quantity,
        unitOfMeasure: ProductFormat.UnitOfMeasure.Code),
      Price: new Price(PriceAmount),
      ImageUrl: new Uri(ProductFormat.ImageUrl, UriKind.Absolute)
  );
  }
}
