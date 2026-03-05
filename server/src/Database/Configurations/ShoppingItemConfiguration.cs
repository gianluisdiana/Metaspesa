using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class ShoppingItemConfiguration : IEntityTypeConfiguration<ShoppingItemDbEntity> {
  public void Configure(EntityTypeBuilder<ShoppingItemDbEntity> builder) {
    builder.ToTable("shopping_items");
    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

    builder.Property(e => e.ShoppingListId)
      .HasColumnName("shopping_list_id")
      .IsRequired();

    builder.Property(e => e.Name)
      .HasColumnName("name")
      .IsRequired();

    builder.Property(e => e.Quantity)
      .HasColumnName("quantity")
      .IsRequired(false);

    builder.Property(e => e.Price)
      .HasColumnName("price")
      .IsRequired(false);

    builder.Property(e => e.IsChecked)
      .HasColumnName("is_checked")
      .IsRequired();
  }
}