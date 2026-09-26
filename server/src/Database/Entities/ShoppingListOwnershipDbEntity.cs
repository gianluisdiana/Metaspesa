namespace Metaspesa.Database.Entities;

internal class ShoppingListOwnershipDbEntity {
  public Guid UserUid { get; set; }
  public Guid ShoppingListId { get; set; }

  public UserDbEntity Owner { get; set; } = null!;
  public ShoppingListDbEntity ShoppingList { get; set; } = null!;
}