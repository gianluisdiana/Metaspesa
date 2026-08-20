using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class ProductFormatConfiguration : IEntityTypeConfiguration<ProductFormatDbEntity> {
  public void Configure(EntityTypeBuilder<ProductFormatDbEntity> builder) {
    builder.ToTable("product_formats", "market", t =>
      t.HasComment("""
      Different package formats for the same product. For example, a soda can be sold
      in 330 ml cans, 500 ml bottles, or 1 l bottles.
      """));

    builder.HasKey(e => e.Id).HasName("pk_product_format");

    builder.Property(e => e.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

    builder.Property(e => e.ProductId)
      .HasColumnName("product_id")
      .IsRequired();

    builder.Property(e => e.Quantity)
      .HasColumnName("quantity")
      .HasPrecision(10, 3)
      .IsRequired();

    builder.Property(e => e.UnitOfMeasureId)
      .HasColumnName("unit_of_measure_id")
      .IsRequired();

    builder.Property(e => e.ImageUrl)
      .HasColumnName("image_url")
      .IsRequired(true);

    builder.HasIndex(e => e.ProductId, "idx_product_format_product_id");
    builder.HasIndex(e => e.UnitOfMeasureId, "idx_product_format_unit_of_measure_id");
    builder.HasAlternateKey(e => new { e.Id, e.ProductId })
      .HasName("ak_product_format_id_product_id");
    builder.HasIndex(
        e => new { e.ProductId, e.Quantity, e.UnitOfMeasureId },
        "idx_product_format_identity")
      .IsUnique();

    builder.ToTable(t => t.HasCheckConstraint(
      "chk_product_format_positive_quantity", "quantity > 0.000"));

    builder.HasOne(e => e.Product)
      .WithMany(e => e.Formats)
      .HasForeignKey(e => e.ProductId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(e => e.UnitOfMeasure)
      .WithMany(e => e.ProductFormats)
      .HasForeignKey(e => e.UnitOfMeasureId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}