using System.Diagnostics;
using Metaspesa.Domain.Markets;

namespace Metaspesa.Database.Entities;

internal class ProductDbEntity {
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public int SuperMarketId { get; set; }
  public int BrandId { get; set; }

  public SuperMarketDbEntity SuperMarket { get; set; } = null!;
  public ProductBrandDbEntity Brand { get; set; } = null!;
  public ICollection<ProductFormatDbEntity> Formats { get; set; } = [];

  public MarketProduct MapToDomain() {
    Debug.Assert(Formats.Any(f => f.PriceSnapshots.Count > 0));
    Debug.Assert(Brand is not null);

    return new MarketProduct(
      Name: Name,
      Brand: new ProductBrand(Brand.Name),
      Formats: [.. Formats
        .Where(f => f.PriceSnapshots.Count > 0)
        .Select(f => f.PriceSnapshots
          .OrderByDescending(s => s.ObservedAt)
          .ThenByDescending(s => s.Id)
          .First()
          .MapToDomainFormat())]);
  }
}
