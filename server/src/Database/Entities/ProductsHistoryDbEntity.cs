using System.Diagnostics;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.Database.Entities;

internal class ProductsHistoryDbEntity {
  public int Id { get; set; }
  public int ProductId { get; set; }
  public int ProductFormatId { get; set; }
  public decimal Price { get; set; }
  public DateTime CreatedAt { get; set; }

  public ProductDbEntity Product { get; set; } = null!;
  public ProductFormatDbEntity ProductFormat { get; set; } = null!;
  public ICollection<ShoppingItemDbEntity> ShoppingItems { get; set; } = [];

  public ProductFormat MapToDomainFormat() {
    Debug.Assert(ProductFormat is not null);

    return new(
      Quantity: new AQuantity(
        value: (float)ProductFormat.Quantity,
        unitOfMeasure: ProductFormat.UnitOfMeasure.Code),
      Price: new Price(Price),
      ImageUrl: new Uri(ProductFormat.ImageUrl, UriKind.Absolute)
  );
  }
}