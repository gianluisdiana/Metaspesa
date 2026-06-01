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
  public ICollection<ProductsHistoryDbEntity> History { get; set; } = [];
  public ICollection<PurchaseDbEntity> Purchases { get; set; } = [];

  public MarketProduct MapToDomain() {
    Debug.Assert(History.Count > 0);
    Debug.Assert(Brand is not null);

    DateTime latestHistoryDate = History.Max(h => h.CreatedAt);

    return new MarketProduct(
      Name: Name,
      Brand: new ProductBrand(Brand.Name),
      Formats: [.. History
        .Where(h => h.CreatedAt == latestHistoryDate)
        .Select(h => h.MapToDomainFormat())]);
  }
}