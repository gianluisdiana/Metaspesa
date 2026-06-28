using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metaspesa.Database.Migrations {
  /// <inheritdoc />
  public partial class DatabasePersistenceRefactor : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
      migrationBuilder.EnsureSchema(name: "purchasing");

      migrationBuilder.DropForeignKey(
          name: "FK_purchase_items_products_history_product_history_id_product_~",
          schema: "shopping",
          table: "purchase_items");
      migrationBuilder.DropForeignKey(
          name: "FK_purchase_items_products_product_id",
          schema: "shopping",
          table: "purchase_items");
      migrationBuilder.DropForeignKey(
          name: "FK_purchase_items_purchases_purchase_id",
          schema: "shopping",
          table: "purchase_items");
      migrationBuilder.DropForeignKey(
          name: "FK_purchases_users_user_uid",
          schema: "shopping",
          table: "purchases");
      migrationBuilder.DropForeignKey(
          name: "FK_shopping_items_product_formats_product_format_id_product_id",
          schema: "shopping",
          table: "shopping_items");
      migrationBuilder.DropForeignKey(
          name: "FK_shopping_items_products_product_id",
          schema: "shopping",
          table: "shopping_items");
      migrationBuilder.DropForeignKey(
          name: "FK_products_history_product_formats_product_format_id_product_~",
          schema: "market",
          table: "products_history");
      migrationBuilder.DropForeignKey(
          name: "FK_products_product_brands_brand_id",
          schema: "market",
          table: "products");
      migrationBuilder.DropForeignKey(
          name: "FK_products_super_markets_super_market_id",
          schema: "market",
          table: "products");
      migrationBuilder.DropForeignKey(
          name: "FK_product_formats_products_product_id",
          schema: "market",
          table: "product_formats");
      migrationBuilder.DropForeignKey(
          name: "FK_product_formats_units_of_measure_unit_of_measure_id",
          schema: "market",
          table: "product_formats");

      migrationBuilder.DropPrimaryKey(
          name: "pk_product_history",
          schema: "market",
          table: "products_history");

      migrationBuilder.DropIndex(
          name: "IX_purchase_items_product_history_id_product_id",
          schema: "shopping",
          table: "purchase_items");
      migrationBuilder.DropIndex(
          name: "idx_purchase_item_product_history_id",
          schema: "shopping",
          table: "purchase_items");
      migrationBuilder.DropIndex(
          name: "idx_purchase_item_product_id",
          schema: "shopping",
          table: "purchase_items");
      migrationBuilder.DropIndex(
          name: "IX_shopping_items_product_format_id_product_id",
          schema: "shopping",
          table: "shopping_items");
      migrationBuilder.DropIndex(
          name: "idx_shopping_item_list_product_format",
          schema: "shopping",
          table: "shopping_items");
      migrationBuilder.DropIndex(
          name: "idx_shopping_item_product_id",
          schema: "shopping",
          table: "shopping_items");
      migrationBuilder.DropIndex(
          name: "IX_products_history_product_format_id_product_id",
          schema: "market",
          table: "products_history");
      migrationBuilder.DropIndex(
          name: "idx_product_history_id_product_id",
          schema: "market",
          table: "products_history");
      migrationBuilder.DropIndex(
          name: "idx_product_history_created_at",
          schema: "market",
          table: "products_history");
      migrationBuilder.DropIndex(
          name: "idx_product_history_product_format_id",
          schema: "market",
          table: "products_history");
      migrationBuilder.DropIndex(
          name: "idx_product_history_product_id",
          schema: "market",
          table: "products_history");

      migrationBuilder.DropUniqueConstraint(
          name: "ak_product_history_id_product_id",
          schema: "market",
          table: "products_history");

      migrationBuilder.RenameTable(
          name: "purchases",
          schema: "shopping",
          newName: "purchases",
          newSchema: "purchasing");
      migrationBuilder.RenameTable(
          name: "purchase_items",
          schema: "shopping",
          newName: "purchase_items",
          newSchema: "purchasing");
      migrationBuilder.RenameTable(
          name: "products_history",
          schema: "market",
          newName: "price_snapshots",
          newSchema: "market");

      migrationBuilder.RenameColumn(
          name: "product_history_id",
          schema: "purchasing",
          table: "purchase_items",
          newName: "price_snapshot_id");
      migrationBuilder.RenameColumn(
          name: "price",
          schema: "market",
          table: "price_snapshots",
          newName: "price_amount");
      migrationBuilder.RenameColumn(
          name: "created_at",
          schema: "market",
          table: "price_snapshots",
          newName: "observed_at");

      migrationBuilder.DropColumn(
          name: "product_id",
          schema: "purchasing",
          table: "purchase_items");
      migrationBuilder.DropColumn(
          name: "product_id",
          schema: "shopping",
          table: "shopping_items");
      migrationBuilder.DropColumn(
          name: "product_id",
          schema: "market",
          table: "price_snapshots");

      migrationBuilder.AddPrimaryKey(
          name: "pk_price_snapshot",
          schema: "market",
          table: "price_snapshots",
          column: "id");

      migrationBuilder.AddColumn<bool>(
          name: "is_temporary",
          schema: "shopping",
          table: "shopping_lists",
          type: "boolean",
          nullable: false,
          defaultValue: false);
      migrationBuilder.AddColumn<string>(
          name: "currency_code",
          schema: "market",
          table: "price_snapshots",
          type: "char(3)",
          nullable: false,
          defaultValue: "EUR");

      migrationBuilder.Sql("""
        UPDATE shopping.shopping_lists
        SET is_temporary = name IS NULL;
        """);

      migrationBuilder.AlterColumn<Guid>(
          name: "user_uid",
          schema: "purchasing",
          table: "purchases",
          type: "uuid",
          nullable: true,
          oldClrType: typeof(Guid),
          oldType: "uuid");

      migrationBuilder.DropCheckConstraint(
          name: "chk_shopping_list_temporary_cannot_be_soft_deleted",
          schema: "shopping",
          table: "shopping_lists");
      migrationBuilder.DropCheckConstraint(
          name: "chk_positive_price",
          schema: "market",
          table: "price_snapshots");

      migrationBuilder.AddCheckConstraint(
          name: "chk_saved_shopping_list_requires_name",
          schema: "shopping",
          table: "shopping_lists",
          sql: "is_temporary = true OR name IS NOT NULL");
      migrationBuilder.AddCheckConstraint(
          name: "chk_shopping_list_temporary_cannot_be_soft_deleted",
          schema: "shopping",
          table: "shopping_lists",
          sql: "is_temporary = false OR deleted_at IS NULL");
      migrationBuilder.AddCheckConstraint(
          name: "chk_price_snapshot_positive_price",
          schema: "market",
          table: "price_snapshots",
          sql: "price_amount > 0.00");
      migrationBuilder.AddCheckConstraint(
          name: "chk_price_snapshot_currency_code",
          schema: "market",
          table: "price_snapshots",
          sql: "currency_code ~ '^[A-Z]{3}$'");

      migrationBuilder.CreateIndex(
          name: "idx_purchase_item_price_snapshot_id",
          schema: "purchasing",
          table: "purchase_items",
          column: "price_snapshot_id");
      migrationBuilder.CreateIndex(
          name: "idx_shopping_item_list_product_format",
          schema: "shopping",
          table: "shopping_items",
          columns: ["shopping_list_id", "product_format_id"]);
      migrationBuilder.CreateIndex(
          name: "idx_shopping_item_list_product_format_active",
          schema: "shopping",
          table: "shopping_items",
          columns: ["shopping_list_id", "product_format_id"],
          unique: true,
          filter: "deleted_at IS NULL");
      migrationBuilder.CreateIndex(
          name: "idx_price_snapshot_observed_at",
          schema: "market",
          table: "price_snapshots",
          column: "observed_at");
      migrationBuilder.CreateIndex(
          name: "idx_price_snapshot_product_format_id",
          schema: "market",
          table: "price_snapshots",
          column: "product_format_id");
      migrationBuilder.CreateIndex(
          name: "idx_price_snapshot_format_observed_at",
          schema: "market",
          table: "price_snapshots",
          columns: ["product_format_id", "observed_at"]);

      migrationBuilder.AddForeignKey(
          name: "FK_purchase_items_price_snapshots_price_snapshot_id",
          schema: "purchasing",
          table: "purchase_items",
          column: "price_snapshot_id",
          principalSchema: "market",
          principalTable: "price_snapshots",
          principalColumn: "id",
          onDelete: ReferentialAction.Restrict);
      migrationBuilder.AddForeignKey(
          name: "FK_purchase_items_purchases_purchase_id",
          schema: "purchasing",
          table: "purchase_items",
          column: "purchase_id",
          principalSchema: "purchasing",
          principalTable: "purchases",
          principalColumn: "id",
          onDelete: ReferentialAction.Cascade);
      migrationBuilder.AddForeignKey(
          name: "FK_purchases_users_user_uid",
          schema: "purchasing",
          table: "purchases",
          column: "user_uid",
          principalSchema: "identity",
          principalTable: "users",
          principalColumn: "uid",
          onDelete: ReferentialAction.SetNull);
      migrationBuilder.AddForeignKey(
          name: "FK_shopping_items_product_formats_product_format_id",
          schema: "shopping",
          table: "shopping_items",
          column: "product_format_id",
          principalSchema: "market",
          principalTable: "product_formats",
          principalColumn: "id",
          onDelete: ReferentialAction.Restrict);
      migrationBuilder.AddForeignKey(
          name: "FK_price_snapshots_product_formats_product_format_id",
          schema: "market",
          table: "price_snapshots",
          column: "product_format_id",
          principalSchema: "market",
          principalTable: "product_formats",
          principalColumn: "id",
          onDelete: ReferentialAction.Restrict);
      migrationBuilder.AddForeignKey(
          name: "FK_products_product_brands_brand_id",
          schema: "market",
          table: "products",
          column: "brand_id",
          principalSchema: "market",
          principalTable: "product_brands",
          principalColumn: "id",
          onDelete: ReferentialAction.Restrict);
      migrationBuilder.AddForeignKey(
          name: "FK_products_super_markets_super_market_id",
          schema: "market",
          table: "products",
          column: "super_market_id",
          principalSchema: "market",
          principalTable: "super_markets",
          principalColumn: "id",
          onDelete: ReferentialAction.Restrict);
      migrationBuilder.AddForeignKey(
          name: "FK_product_formats_products_product_id",
          schema: "market",
          table: "product_formats",
          column: "product_id",
          principalSchema: "market",
          principalTable: "products",
          principalColumn: "id",
          onDelete: ReferentialAction.Restrict);
      migrationBuilder.AddForeignKey(
          name: "FK_product_formats_units_of_measure_unit_of_measure_id",
          schema: "market",
          table: "product_formats",
          column: "unit_of_measure_id",
          principalSchema: "market",
          principalTable: "units_of_measure",
          principalColumn: "id",
          onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.Sql("-- Irreversible data-shape refactor.");
    }
  }
}
