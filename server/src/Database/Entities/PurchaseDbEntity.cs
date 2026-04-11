namespace Metaspesa.Database.Entities;

internal class PurchaseDbEntity {
  public int Id { get; set; }
  public Guid UserUid { get; set; }
  public int RegisteredItemId { get; set; }
  public int? ProductId { get; set; }
  public int? SuperMarketId { get; set; }
  public float PricePaid { get; set; }
  public string? Quantity { get; set; }
  public DateTime PurchasedAt { get; set; }

  public UserDbEntity User { get; set; } = null!;
  public RegisteredItemDbEntity RegisteredItem { get; set; } = null!;
  public ProductDbEntity? Product { get; set; }
  public SuperMarketDbEntity? SuperMarket { get; set; }
}