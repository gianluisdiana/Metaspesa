namespace Metaspesa.Database.Entities;

internal class SuperMarketDbEntity {
  public int Id { get; set; }
  public string Name { get; set; } = null!;
  public string? LogoUrl { get; set; }

  public ICollection<ProductDbEntity> Products { get; set; } = [];
  public ICollection<PurchaseDbEntity> Purchases { get; set; } = [];
}