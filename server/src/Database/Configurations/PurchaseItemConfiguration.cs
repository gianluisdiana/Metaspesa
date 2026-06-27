using Metaspesa.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metaspesa.Database.Configurations;

internal class PurchaseItemConfiguration : IEntityTypeConfiguration<PurchaseItemDbEntity> {
  public void Configure(EntityTypeBuilder<PurchaseItemDbEntity> builder) {
    builder.ToTable("purchase_items", "shopping", t =>
      t.HasComment("""
      Immutable purchase receipt lines. Each line references the exact market
      product history row used for analytics, which provides the exact price and
      format paid at purchase time.
      """));

    builder.HasKey(e => e.Id).HasName("pk_purchase_item");

    builder.Property(e => e.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

    builder.Property(e => e.PurchaseId)
      .HasColumnName("purchase_id")
      .IsRequired();

    builder.Property(e => e.ProductId)
      .HasColumnName("product_id")
      .IsRequired();

    builder.Property(e => e.ProductHistoryId)
      .HasColumnName("product_history_id")
      .IsRequired();

    builder.Property(e => e.Amount)
      .HasColumnName("amount")
      .HasDefaultValue(1)
      .IsRequired()
      .HasComment("How many units/packages were bought");

    builder.HasIndex(e => e.PurchaseId, "idx_purchase_item_purchase_id");
    builder.HasIndex(e => e.ProductId, "idx_purchase_item_product_id");
    builder.HasIndex(e => e.ProductHistoryId, "idx_purchase_item_product_history_id");

    builder.ToTable(t => t.HasCheckConstraint(
      "chk_purchase_item_positive_amount", "amount > 0"));

    builder.HasOne(e => e.Purchase)
      .WithMany(e => e.Items)
      .HasForeignKey(e => e.PurchaseId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(e => e.Product)
      .WithMany(e => e.PurchaseItems)
      .HasForeignKey(e => e.ProductId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(e => e.ProductHistory)
      .WithMany(e => e.PurchaseItems)
      .HasForeignKey(e => new { e.ProductHistoryId, e.ProductId })
      .HasPrincipalKey(e => new { e.Id, e.ProductId })
      .OnDelete(DeleteBehavior.Restrict);
  }
}