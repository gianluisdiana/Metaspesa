using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class ShoppingListOwnershipConfiguration : IEntityTypeConfiguration<ShoppingListOwnershipDbEntity>
{
  public void Configure(EntityTypeBuilder<ShoppingListOwnershipDbEntity> builder)
  {
    builder.ToTable("shopping_list_ownerships");
    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

    builder.Property(e => e.LastTimeUsed)
      .HasColumnName("last_time_used")
      .IsRequired();

    builder.Property(e => e.UserUid)
      .HasColumnName("user_uid")
      .IsRequired();

    builder.Property(e => e.ShoppingListId)
      .HasColumnName("shopping_list_id")
      .IsRequired();
  }
}