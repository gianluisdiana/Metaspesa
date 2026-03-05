using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class ShoppingListConfiguration : IEntityTypeConfiguration<ShoppingListDbEntity>
{
  public void Configure(EntityTypeBuilder<ShoppingListDbEntity> builder)
  {
    builder.ToTable("shopping_lists");
    builder.HasKey(e => e.Id);

    builder.Property(e => e.Name)
      .HasColumnName("name")
      .IsRequired(false);

    builder.Property(e => e.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

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