using Metaspesa.Database.Entities;
using Metaspesa.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class UserRoleConfiguration : IEntityTypeConfiguration<UserRoleDbEntity> {
  public void Configure(EntityTypeBuilder<UserRoleDbEntity> builder) {
    builder.ToTable("roles", "shopping", t =>
      t.HasComment("User roles for access control"));
    builder.HasKey(e => e.Id).HasName("pk_role");

    builder.Property(e => e.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

    builder.Property(e => e.Name)
      .HasColumnName("name")
      .HasMaxLength(100)
      .IsRequired();

    builder.HasIndex(e => e.Name, "idx_role_name")
      .IsUnique();

    builder.Property(e => e.Description)
      .HasColumnName("description")
      .HasMaxLength(500)
      .IsRequired();

    builder.HasData(
      new UserRoleDbEntity { Id = 1, Name = nameof(Role.Shopper), Description = "Regular user who manages shopping lists" },
      new UserRoleDbEntity { Id = 2, Name = nameof(Role.ProductManager), Description = "User who manages market products" }
    );
  }
}
