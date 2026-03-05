namespace Metaspesa.Database.Entities;

internal class RegisteredItemsHistoryDbEntity {
  public int Id { get; set; }
  public int RegisteredItemId { get; set; }
  public float? Price { get; set; }
  public DateTime CreatedAt { get; set; }

  public RegisteredItemDbEntity RegisteredItem { get; set; } = null!;
}
