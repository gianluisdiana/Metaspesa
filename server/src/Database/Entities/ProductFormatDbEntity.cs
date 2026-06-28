namespace Metaspesa.Database.Entities;

internal class ProductFormatDbEntity {
  public int Id { get; set; }
  public int ProductId { get; set; }
  public decimal Quantity { get; set; }
  public int UnitOfMeasureId { get; set; }
  public string ImageUrl { get; set; } = null!;

  public ProductDbEntity Product { get; set; } = null!;
  public UnitOfMeasureDbEntity UnitOfMeasure { get; set; } = null!;
  public ICollection<ProductsHistoryDbEntity> History { get; set; } = [];
  public ICollection<ShoppingItemDbEntity> ShoppingItems { get; set; } = [];
}