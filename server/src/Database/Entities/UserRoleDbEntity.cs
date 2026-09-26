namespace Metaspesa.Database.Entities;

internal class UserRoleDbEntity {
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;

  public ICollection<UserDbEntity> Users { get; set; } = [];
}