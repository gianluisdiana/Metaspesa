namespace Metaspesa.Database.Entities;

internal class ShoppingItemDbEntity {
  public int Id { get; set; }
  public int ShoppingListId { get; set; }
  public string Name { get; set; } = null!;
  public string? Quantity { get; set; }
  public float? Price { get; set; }
  public bool IsChecked { get; set; }

  public ShoppingListDbEntity ShoppingList { get; set; } = null!;
}
