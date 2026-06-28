namespace Metaspesa.Database.Entities;

internal class ShoppingItemDbEntity {
  public int Id { get; set; }
  public int ShoppingListId { get; set; }
  public int ProductFormatId { get; set; }
  public int Amount { get; set; } = 1;
  public bool IsChecked { get; set; }
  public DateTime? DeletedAt { get; set; }

  public ShoppingListDbEntity ShoppingList { get; set; } = null!;
  public ProductFormatDbEntity ProductFormat { get; set; } = null!;
}