using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class ShoppingListConfiguration : IEntityTypeConfiguration<ShoppingListDbEntity> {
  public void Configure(EntityTypeBuilder<ShoppingListDbEntity> builder) {
    builder.ToTable("shopping_lists", "shopping", t =>
      t.HasComment("Shopping lists created by users, can be shared among multiple users")
        .HasCheckConstraint(
          "chk_shopping_list_temporary_cannot_be_soft_deleted",
          "name IS NOT NULL OR deleted_at IS NULL"
        )
      );

    builder.HasKey(e => e.Id).HasName("pk_shopping_list");

    builder.Property(e => e.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

    builder.Property(e => e.Name)
      .HasColumnName("name")
      .HasComment("Missing name indicates a temporary shopping list, not saved by the user")
      .IsRequired(false);

    builder.Property(e => e.DeletedAt)
      .HasColumnName("deleted_at")
      .IsRequired(false);

    builder.HasMany(e => e.Items)
      .WithOne(e => e.ShoppingList)
      .HasForeignKey(e => e.ShoppingListId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(e => e.Ownerships)
      .WithOne(e => e.ShoppingList)
      .HasForeignKey(e => e.ShoppingListId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}