namespace Metaspesa.Database.Entities;

internal class ShoppingListOwnershipDbEntity {
  public Guid UserUid { get; set; }
  public int ShoppingListId { get; set; }

  public UserDbEntity Owner { get; set; } = null!;
  public ShoppingListDbEntity ShoppingList { get; set; } = null!;
}