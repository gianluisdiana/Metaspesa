namespace Metaspesa.Database.Entities;

internal class ShoppingListDbEntity {
  public int Id { get; set; }
  public string? Name { get; set; }
  public DateTime? DeletedAt { get; set; }

  public ICollection<ShoppingItemDbEntity> Items { get; set; } = [];
  public ICollection<ShoppingListOwnershipDbEntity> Ownerships { get; set; } = [];
}