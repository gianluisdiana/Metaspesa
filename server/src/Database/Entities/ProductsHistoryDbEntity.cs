namespace Metaspesa.Database.Entities;

internal class ProductsHistoryDbEntity {
  public int Id { get; set; }
  public float Price { get; set; }
  public string Quantity { get; set; } = null!;
  public string? ImageUrl { get; set; }
  public DateTime CreatedAt { get; set; }
  public int ProductId { get; set; }

  public ProductDbEntity Product { get; set; } = null!;
}