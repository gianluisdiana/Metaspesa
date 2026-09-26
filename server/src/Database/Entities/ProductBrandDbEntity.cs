namespace Metaspesa.Database.Entities;

internal class ProductBrandDbEntity {
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;

  public ICollection<ProductDbEntity> Products { get; set; } = [];
}