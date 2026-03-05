using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class RegisteredItemsHistoryConfiguration : IEntityTypeConfiguration<RegisteredItemsHistoryDbEntity> {
  public void Configure(EntityTypeBuilder<RegisteredItemsHistoryDbEntity> builder) {
    builder.ToTable("registered_items_history");
    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

    builder.Property(e => e.RegisteredItemId)
      .HasColumnName("registered_item_id")
      .IsRequired();

    builder.Property(e => e.Price)
      .HasColumnName("price")
      .IsRequired(false);

    builder.Property(e => e.CreatedAt)
      .HasColumnName("created_at")
      .HasDefaultValueSql("CURRENT_TIMESTAMP")
      .IsRequired();
  }
}