namespace Metaspesa.Database.Entities;

internal class PurchaseDbEntity {
  public Guid Id { get; set; }
  public Guid? UserUid { get; set; }
  public Guid? ShoppingListId { get; set; }
  public DateTime PurchasedAt { get; set; }

  public UserDbEntity? User { get; set; }
  public ShoppingListDbEntity? ShoppingList { get; set; }
  public ICollection<PurchaseItemDbEntity> Items { get; set; } = [];
}