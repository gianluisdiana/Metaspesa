using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class ShoppingListOwnershipConfiguration : IEntityTypeConfiguration<ShoppingListOwnershipDbEntity> {
  public void Configure(EntityTypeBuilder<ShoppingListOwnershipDbEntity> builder) {
    builder.ToTable("shopping_list_ownerships", "shopping", t =>
      t.HasComment("Associates users with shopping lists, allowing for shared lists"));

    builder.HasKey(e => new { e.UserUid, e.ShoppingListId })
      .HasName("pk_shopping_list_ownership");

    builder.Property(e => e.UserUid)
      .HasColumnName("user_uid")
      .IsRequired();

    builder.Property(e => e.ShoppingListId)
      .HasColumnName("shopping_list_id")
      .IsRequired();

    builder.HasIndex(e => e.UserUid, "idx_shopping_list_ownership_user_uid");
    builder.HasIndex(e => e.ShoppingListId, "idx_shopping_list_ownership_shopping_list_id");
  }
}