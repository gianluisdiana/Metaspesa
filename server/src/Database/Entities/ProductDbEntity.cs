namespace Metaspesa.Database.Entities;

internal class ProductDbEntity {
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public string? Image { get; set; }
  public int SuperMarketId { get; set; }
  public int BrandId { get; set; }

  public SuperMarketDbEntity SuperMarket { get; set; } = null!;
  public ProductBrandDbEntity Brand { get; set; } = null!;
  public ICollection<ProductsHistoryDbEntity> History { get; set; } = [];
  public ICollection<PurchaseDbEntity> Purchases { get; set; } = [];
}