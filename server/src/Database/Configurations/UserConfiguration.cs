using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<UserDbEntity> {
  public void Configure(EntityTypeBuilder<UserDbEntity> builder) {
    builder.ToTable("users");
    builder.HasKey(e => e.Uid);

    builder.HasIndex(e => e.Username).IsUnique();

    builder.Property(e => e.Uid)
      .HasColumnName("uid")
      .ValueGeneratedOnAdd();

    builder.Property(e => e.Username)
      .HasColumnName("username")
      .IsRequired();

    builder.HasMany(e => e.RegisteredItems)
      .WithOne(e => e.User)
      .HasForeignKey(e => e.UserUid)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
