using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasureDbEntity> {
  public void Configure(EntityTypeBuilder<UnitOfMeasureDbEntity> builder) {
    builder.ToTable("units_of_measure", "market", t =>
      t.HasComment("""
      Units used to describe product package format, not shopping count.
      Examples: ml, l, g, kg, piece.
      """));

    builder.HasKey(e => e.Id).HasName("pk_unit_of_measure");

    builder.Property(e => e.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

    builder.Property(e => e.Code)
      .HasColumnName("code")
      .HasMaxLength(16)
      .IsRequired()
      .HasComment("Stable unit code, e.g. ml, kg, piece");

    builder.Property(e => e.Name)
      .HasColumnName("name")
      .HasMaxLength(64)
      .IsRequired();

    builder.HasIndex(e => e.Code, "idx_unit_of_measure_code").IsUnique();
  }
}