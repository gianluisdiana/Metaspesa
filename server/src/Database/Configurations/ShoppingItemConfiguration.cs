using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class ShoppingItemConfiguration : IEntityTypeConfiguration<ShoppingItemDbEntity> {
  public void Configure(EntityTypeBuilder<ShoppingItemDbEntity> builder) {
    builder.ToTable("shopping_items", "shopping", t =>
      t.HasComment("Items that belong to a shopping list, representing planned purchases"));

    builder.HasKey(e => e.Id).HasName("pk_shopping_item");

    builder.Property(e => e.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

    builder.Property(e => e.ShoppingListId)
      .HasColumnName("shopping_list_id")
      .IsRequired();

    builder.Property(e => e.Name)
      .HasColumnName("name")
      .IsRequired();

    builder.Property(e => e.Price)
      .HasColumnName("price")
      .IsRequired();

    builder.Property(e => e.Quantity)
      .HasColumnName("quantity")
      .IsRequired(false);

    builder.Property(e => e.DeletedAt)
      .HasColumnName("deleted_at")
      .IsRequired(false);

    builder.Property(e => e.IsChecked)
      .HasColumnName("is_checked")
      .IsRequired();

    builder.HasIndex(e => e.ShoppingListId, "idx_shopping_item_shopping_list_id");

    builder.ToTable(t => t.HasCheckConstraint(
      "chk_shopping_item_positive_price", "price >= 0.00"));
  }
}