namespace Metaspesa.Database.Entities;

internal class UserDbEntity {
  public Guid Uid { get; set; }
  public string Username { get; set; } = null!;

  public ICollection<RegisteredItemDbEntity> RegisteredItems { get; set; } = [];
  public ICollection<ShoppingListOwnershipDbEntity> ShoppingListOwnerships { get; set; } = [];
}