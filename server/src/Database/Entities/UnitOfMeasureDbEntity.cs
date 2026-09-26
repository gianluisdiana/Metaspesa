namespace Metaspesa.Database.Entities;

internal class UnitOfMeasureDbEntity {
  public Guid Id { get; set; }
  public string Code { get; set; } = null!;
  public string Name { get; set; } = null!;

  public ICollection<ProductFormatDbEntity> ProductFormats { get; set; } = [];
}