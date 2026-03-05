namespace Metaspesa.Database.Entities;

internal class ShoppingListOwnershipDbEntity {
  public int Id { get; set; }
  public Guid UserUid { get; set; }
  public int ShoppingListId { get; set; }
  public DateTime LastTimeUsed { get; set; }

  public UserDbEntity User { get; set; } = null!;
  public ShoppingListDbEntity ShoppingList { get; set; } = null!;
}