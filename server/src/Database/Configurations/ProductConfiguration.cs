using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class ProductConfiguration : IEntityTypeConfiguration<ProductDbEntity> {
  public void Configure(EntityTypeBuilder<ProductDbEntity> builder) {
    builder.ToTable("products", "market", t =>
      t.HasComment("Products available in the market"));

    builder.HasKey(e => e.Id).HasName("pk_product");

    builder.Property(e => e.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

    builder.Property(e => e.Name)
      .HasColumnName("name")
      .IsRequired();

    builder.Property(e => e.Image)
      .HasColumnName("image")
      .IsRequired(false);

    builder.Property(e => e.SuperMarketId)
      .HasColumnName("super_market_id")
      .IsRequired();

    builder.Property(e => e.BrandId)
      .HasColumnName("brand_id")
      .IsRequired();

    builder.HasIndex(e => e.SuperMarketId, "idx_product_super_market_id");
    builder.HasIndex(e => e.BrandId, "idx_product_brand_id");
    builder.HasIndex(
        e => new { e.Name, e.SuperMarketId, e.BrandId },
        "idx_product_name_super_market_brand")
      .IsUnique();

    builder.HasOne(e => e.SuperMarket)
      .WithMany(e => e.Products)
      .HasForeignKey(e => e.SuperMarketId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(e => e.Brand)
      .WithMany(e => e.Products)
      .HasForeignKey(e => e.BrandId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(e => e.History)
      .WithOne(e => e.Product)
      .HasForeignKey(e => e.ProductId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}