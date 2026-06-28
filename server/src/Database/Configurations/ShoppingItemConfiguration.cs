using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class ShoppingItemConfiguration : IEntityTypeConfiguration<ShoppingItemDbEntity> {
  public void Configure(EntityTypeBuilder<ShoppingItemDbEntity> builder) {
    builder.ToTable("shopping_items", "shopping", t =>
      t.HasComment("""
      Items that belong to a shopping list, representing planned purchases.
      Each line points to an existing product format.
      """));

    builder.HasKey(e => e.Id).HasName("pk_shopping_item");

    builder.Property(e => e.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

    builder.Property(e => e.ShoppingListId)
      .HasColumnName("shopping_list_id")
      .IsRequired();

    builder.Property(e => e.ProductFormatId)
      .HasColumnName("product_format_id")
      .IsRequired();

    builder.Property(e => e.Amount)
      .HasColumnName("amount")
      .HasDefaultValue(1)
      .IsRequired()
      .HasComment("How many units/packages of the product are planned");

    builder.Property(e => e.IsChecked)
      .HasColumnName("is_checked")
      .HasDefaultValue(false)
      .IsRequired();

    builder.Property(e => e.DeletedAt)
      .HasColumnName("deleted_at")
      .IsRequired(false);

    builder.HasIndex(e => e.ShoppingListId, "idx_shopping_item_shopping_list_id");
    builder.HasIndex(e => e.ProductFormatId, "idx_shopping_item_product_format_id");
    builder.HasIndex(
        e => new { e.ShoppingListId, e.ProductFormatId },
        "idx_shopping_item_list_product_format");
    builder.HasIndex(
        e => new { e.ShoppingListId, e.ProductFormatId },
        "idx_shopping_item_list_product_format_active")
      .IsUnique()
      .HasFilter("deleted_at IS NULL");

    builder.ToTable(t => t.HasCheckConstraint(
      "chk_shopping_item_positive_amount", "amount > 0"));

    builder.HasOne(e => e.ShoppingList)
      .WithMany(e => e.Items)
      .HasForeignKey(e => e.ShoppingListId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(e => e.ProductFormat)
      .WithMany(e => e.ShoppingItems)
      .HasForeignKey(e => e.ProductFormatId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
