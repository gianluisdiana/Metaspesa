using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class RegisteredItemConfiguration : IEntityTypeConfiguration<RegisteredItemDbEntity> {
  public void Configure(EntityTypeBuilder<RegisteredItemDbEntity> builder) {
    builder.ToTable("registered_items");
    builder.HasKey(e => e.Id);

    builder.HasIndex(e => e.Name).IsUnique();

    builder.Property(e => e.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

    builder.Property(e => e.UserUid)
      .HasColumnName("user_id")
      .IsRequired();
    builder.Property(e => e.Name)
      .HasColumnName("name")
      .IsRequired();
    builder.Property(e => e.Quantity)
      .HasColumnName("quantity")
      .IsRequired(false);

    builder.HasMany(e => e.History)
      .WithOne(e => e.RegisteredItem)
      .HasForeignKey(e => e.RegisteredItemId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
