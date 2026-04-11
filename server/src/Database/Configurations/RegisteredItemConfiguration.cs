using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class RegisteredItemConfiguration : IEntityTypeConfiguration<RegisteredItemDbEntity> {
  public void Configure(EntityTypeBuilder<RegisteredItemDbEntity> builder) {
    builder.ToTable("registered_items", "shopping", t =>
      t.HasComment("Items that users have registered as purchased or planned to purchase"));

    builder.HasKey(e => e.Id).HasName("pk_registered_item");

    builder.Property(e => e.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

    builder.Property(e => e.UserUid)
      .HasColumnName("user_uid")
      .IsRequired();

    builder.Property(e => e.Name)
      .HasColumnName("name")
      .IsRequired();

    builder.Property(e => e.LastKnownPrice)
      .HasColumnName("last_known_price")
      .IsRequired();

    builder.Property(e => e.Quantity)
      .HasColumnName("quantity")
      .IsRequired(false);

    builder.HasIndex(e => e.UserUid, "idx_registered_item_user_uid");

    builder.ToTable(t => t.HasCheckConstraint(
      "chk_positive_last_known_price", "last_known_price >= 0.00"));

    builder.HasMany(e => e.Purchases)
      .WithOne(e => e.RegisteredItem)
      .HasForeignKey(e => e.RegisteredItemId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}