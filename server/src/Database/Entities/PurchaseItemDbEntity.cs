namespace Metaspesa.Database.Entities;

internal class PurchaseItemDbEntity {
  public int Id { get; set; }
  public int PurchaseId { get; set; }
  public int ProductId { get; set; }
  public int ProductHistoryId { get; set; }
  public int Amount { get; set; } = 1;

  public PurchaseDbEntity Purchase { get; set; } = null!;
  public ProductDbEntity Product { get; set; } = null!;
  public ProductsHistoryDbEntity ProductHistory { get; set; } = null!;
}