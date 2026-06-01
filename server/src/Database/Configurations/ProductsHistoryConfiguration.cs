using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class ProductsHistoryConfiguration : IEntityTypeConfiguration<ProductsHistoryDbEntity> {
  public void Configure(EntityTypeBuilder<ProductsHistoryDbEntity> builder) {
    builder.ToTable("products_history", "market", t =>
      t.HasComment("""
      Historical price and format data for products, tracking changes over time.
      """));

    builder.HasKey(e => e.Id).HasName("pk_product_history");

    builder.Property(e => e.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

    builder.Property(e => e.ProductId)
      .HasColumnName("product_id")
      .IsRequired();

    builder.Property(e => e.ProductFormatId)
      .HasColumnName("product_format_id")
      .IsRequired();

    builder.Property(e => e.Price)
      .HasColumnName("price")
      .HasPrecision(10, 2)
      .IsRequired();

    builder.Property(e => e.CreatedAt)
      .HasColumnName("created_at")
      .HasDefaultValueSql("now()")
      .IsRequired();

    builder.ToTable(t => t.HasCheckConstraint(
      "chk_positive_price", "price > 0.00"));

    builder.HasIndex(e => e.ProductId, "idx_product_history_product_id");
    builder.HasIndex(e => e.ProductFormatId, "idx_product_history_product_format_id");
    builder.HasIndex(e => e.CreatedAt, "idx_product_history_created_at");
    builder.HasAlternateKey(e => new { e.Id, e.ProductId })
      .HasName("ak_product_history_id_product_id");
    builder.HasIndex(e => new { e.Id, e.ProductId }, "idx_product_history_id_product_id")
      .IsUnique();

    builder.HasOne(e => e.Product)
      .WithMany(e => e.History)
      .HasForeignKey(e => e.ProductId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(e => e.ProductFormat)
      .WithMany(e => e.History)
      .HasForeignKey(e => new { e.ProductFormatId, e.ProductId })
      .HasPrincipalKey(e => new { e.Id, e.ProductId })
      .OnDelete(DeleteBehavior.Cascade);
  }
}