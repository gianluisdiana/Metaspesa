namespace Metaspesa.Database.Entities;

internal class RegisteredItemDbEntity {
  public int Id { get; set; }
  public Guid UserUid { get; set; }
  public string Name { get; set; } = null!;
  public float LastKnownPrice { get; set; }
  public string? Quantity { get; set; }

  public UserDbEntity User { get; set; } = null!;
  public ICollection<PurchaseDbEntity> Purchases { get; set; } = [];
}