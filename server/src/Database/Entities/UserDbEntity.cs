namespace Metaspesa.Database.Entities;

internal class UserDbEntity {
  public Guid Uid { get; set; }
  public string Username { get; set; } = null!;
  public string EncryptedPassword { get; set; } = null!;
  public int RoleId { get; set; }

  public UserRoleDbEntity Role { get; set; } = null!;
  public ICollection<RegisteredItemDbEntity> RegisteredItems { get; set; } = [];
  public ICollection<ShoppingListOwnershipDbEntity> Ownerships { get; set; } = [];
  public ICollection<PurchaseDbEntity> Purchases { get; set; } = [];
}