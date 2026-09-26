namespace Metaspesa.Database.Entities;

internal class ShoppingItemDbEntity {
  public Guid Id { get; set; }
  public Guid ShoppingListId { get; set; }
  public Guid ProductFormatId { get; set; }
  public int Amount { get; set; } = 1;
  public bool IsChecked { get; set; }
  public DateTime? DeletedAt { get; set; }

  public ShoppingListDbEntity ShoppingList { get; set; } = null!;
  public ProductFormatDbEntity ProductFormat { get; set; } = null!;
}