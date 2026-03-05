namespace Metaspesa.Database.Entities;

internal class RegisteredItemDbEntity {
  public int Id { get; set; }
  public Guid UserUid { get; set; }
  public string Name { get; set; } = null!;
  public string? Quantity { get; set; }

  public ICollection<RegisteredItemsHistoryDbEntity> History { get; set; } = [];
  public UserDbEntity User { get; set; } = null!;
}
