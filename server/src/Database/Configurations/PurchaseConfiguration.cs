using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class PurchaseConfiguration : IEntityTypeConfiguration<PurchaseDbEntity> {
  public void Configure(EntityTypeBuilder<PurchaseDbEntity> builder) {
    builder.ToTable("purchases", "shopping", t =>
      t.HasComment("""
      Records the actual act of buying an item — links shopping, registered
      items, and the market. Core of savings analytics.
      """));

    builder.HasKey(e => e.Id).HasName("pk_purchase");

    builder.Property(e => e.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

    builder.Property(e => e.UserUid)
      .HasColumnName("user_uid")
      .IsRequired();

    builder.Property(e => e.RegisteredItemId)
      .HasColumnName("registered_item_id")
      .IsRequired();

    builder.Property(e => e.ProductId)
      .HasColumnName("product_id")
      .IsRequired(false);

    builder.Property(e => e.SuperMarketId)
      .HasColumnName("super_market_id")
      .IsRequired(false);

    builder.Property(e => e.PricePaid)
      .HasColumnName("price_paid")
      .IsRequired();

    builder.Property(e => e.Quantity)
      .HasColumnName("quantity")
      .IsRequired(false);

    builder.Property(e => e.PurchasedAt)
      .HasColumnName("purchased_at")
      .HasDefaultValueSql("now()")
      .IsRequired();

    builder.ToTable(t => t.HasCheckConstraint(
      "chk_positive_price_paid", "price_paid >= 0.00"));

    builder.HasIndex(e => e.UserUid, "idx_purchase_user_uid");
    builder.HasIndex(e => e.RegisteredItemId, "idx_purchase_registered_item_id");
    builder.HasIndex(e => e.ProductId, "idx_purchase_product_id");
    builder.HasIndex(e => e.SuperMarketId, "idx_purchase_super_market_id");
    builder.HasIndex(e => e.PurchasedAt, "idx_purchase_purchased_at");

    builder.HasOne(e => e.Product)
      .WithMany(e => e.Purchases)
      .HasForeignKey(e => e.ProductId)
      .OnDelete(DeleteBehavior.SetNull);

    builder.HasOne(e => e.SuperMarket)
      .WithMany(e => e.Purchases)
      .HasForeignKey(e => e.SuperMarketId)
      .OnDelete(DeleteBehavior.SetNull);
  }
}