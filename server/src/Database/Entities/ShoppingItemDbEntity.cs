namespace Metaspesa.Database.Entities;

internal class ShoppingItemDbEntity {
  public int Id { get; set; }
  public int ShoppingListId { get; set; }
  public int ProductId { get; set; }
  public int ProductHistoryId { get; set; }
  public int Amount { get; set; } = 1;
  public bool IsChecked { get; set; }
  public DateTime? DeletedAt { get; set; }

  public ShoppingListDbEntity ShoppingList { get; set; } = null!;
  public ProductDbEntity Product { get; set; } = null!;
  public ProductsHistoryDbEntity ProductHistory { get; set; } = null!;
}