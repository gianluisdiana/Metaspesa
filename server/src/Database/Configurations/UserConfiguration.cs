using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<UserDbEntity> {
  public void Configure(EntityTypeBuilder<UserDbEntity> builder) {
    builder.ToTable("users", "identity", t =>
      t.HasComment("Registered users of the shopping application"));
    builder.HasKey(e => e.Uid).HasName("pk_user");

    builder.HasIndex(e => e.Username, "idx_user_username")
      .IsUnique();

    builder.Property(e => e.Uid)
      .HasColumnName("uid")
      .ValueGeneratedNever();

    builder.Property(e => e.Username)
      .HasColumnName("username")
      .HasMaxLength(255)
      .IsRequired();

    builder.Property(e => e.EncryptedPassword)
      .HasColumnName("encrypted_password")
      .HasMaxLength(255)
      .IsRequired();

    builder.Property(e => e.RoleId)
      .HasColumnName("role_id")
      .IsRequired();

    builder.HasIndex(e => e.RoleId, "idx_user_role_id");

    builder.HasOne(e => e.Role)
      .WithMany(r => r.Users)
      .HasForeignKey(e => e.RoleId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasMany(e => e.Purchases)
      .WithOne(e => e.User)
      .HasForeignKey(e => e.UserUid)
      .OnDelete(DeleteBehavior.SetNull);

    builder.HasMany(e => e.Ownerships)
      .WithOne(e => e.Owner)
      .HasForeignKey(e => e.UserUid)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
