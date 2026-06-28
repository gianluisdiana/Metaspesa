namespace Metaspesa.Database.Entities;

internal class PurchaseItemDbEntity {
  public int Id { get; set; }
  public int PurchaseId { get; set; }
  public int PriceSnapshotId { get; set; }
  public int Amount { get; set; } = 1;

  public PurchaseDbEntity Purchase { get; set; } = null!;
  public PriceSnapshotDbEntity PriceSnapshot { get; set; } = null!;
}