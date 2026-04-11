using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class ProductsHistoryConfiguration : IEntityTypeConfiguration<ProductsHistoryDbEntity> {
  public void Configure(EntityTypeBuilder<ProductsHistoryDbEntity> builder) {
    builder.ToTable("products_history", "market", t =>
      t.HasComment("Historical price and quantity data for products, tracking changes over time"));

    builder.HasKey(e => e.Id).HasName("pk_product_history");

    builder.Property(e => e.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

    builder.Property(e => e.Price)
      .HasColumnName("price")
      .IsRequired();

    builder.Property(e => e.Quantity)
      .HasColumnName("quantity")
      .IsRequired();

    builder.Property(e => e.CreatedAt)
      .HasColumnName("created_at")
      .HasDefaultValueSql("now()")
      .IsRequired();

    builder.Property(e => e.ProductId)
      .HasColumnName("product_id")
      .IsRequired();

    builder.ToTable(t => t.HasCheckConstraint(
      "chk_positive_price", "price >= 0.00"));

    builder.HasIndex(e => e.ProductId, "idx_product_history_product_id");
    builder.HasIndex(e => e.CreatedAt, "idx_product_history_created_at");
  }
}