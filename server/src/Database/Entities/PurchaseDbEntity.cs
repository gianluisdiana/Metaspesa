namespace Metaspesa.Database.Entities;

internal class PurchaseDbEntity {
  public int Id { get; set; }
  public Guid UserUid { get; set; }
  public int? ShoppingListId { get; set; }
  public DateTime PurchasedAt { get; set; }

  public UserDbEntity User { get; set; } = null!;
  public ShoppingListDbEntity? ShoppingList { get; set; }
  public ICollection<PurchaseItemDbEntity> Items { get; set; } = [];
}